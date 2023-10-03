using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using SpaceAce.Architecture;
using SpaceAce.Auxiliary;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace SpaceAce.Main.Audio
{
    [CreateAssetMenu(fileName = "Audio collection", menuName = "Space ace/Configs/Audio collection")]
    public sealed class AudioCollection : ScriptableObject, IMusic
    {
        public const float MinSpatialBlend = 0f;
        public const float MaxSpatialBlend = 1f;
        public const float DefaultSpatialBlend = 0.5f;

        public const float MinPitch = 0.5f;
        public const float MaxPitch = 2f;
        public const float DefaultPitch = 1f;

        private static readonly CachedService<AudioPlayer> s_audioPlayer = new();

        private int _nextAudioClipIndex = 0;
        private Queue<int> _nonRepeatingAudioClipsIndices;

        [SerializeField]
        private List<AudioClip> _audioClips;

        [SerializeField]
        private AudioMixerGroup _outputAudioGroup;

        [SerializeField, MinMaxSlider(0f, 1f)]
        private Vector2 _volume;

        [SerializeField]
        private AudioClipPriority _priority = AudioClipPriority.Lowest;

        [SerializeField, MinMaxSlider(MinSpatialBlend, MaxSpatialBlend)]
        private Vector2 _spatialBlend;

        [SerializeField, MinMaxSlider(MinPitch, MaxPitch)]
        private Vector2 _pitch;

        public AudioMixerGroup OutputAudioGroup => _outputAudioGroup;

        public float RandomVolume => AuxMath.GetRandom(_volume.x, _volume.y);

        public AudioClipPriority Priority => _priority;

        public float RandomSpatialBlend => AuxMath.GetRandom(_spatialBlend.x, _spatialBlend.y);

        public float RandomPitch => AuxMath.GetRandom(_pitch.x, _pitch.y);

        public AudioClip RandomAudioClip => _audioClips[Random.Range(0, _audioClips.Count)];

        public AudioClip NonRepeatingRandomAudioClip
        {
            get
            {
                if (_nonRepeatingAudioClipsIndices.Count == 0)
                    ReplenishNonRepeatingAudioClipsIndices();

                int index = _nonRepeatingAudioClipsIndices.Dequeue();

                return _audioClips[index];
            }
        }

        public AudioClip NextAudioClip => _audioClips[_nextAudioClipIndex++ % _audioClips.Count];

        public int AudioClipsAmount => _audioClips.Count;

        private void OnEnable()
        {
            if (_audioClips is not null) _nonRepeatingAudioClipsIndices = new(_audioClips.Count);
        }

        public AudioAccess PlayRandomAudioClip(Vector3 position) => PlayAudioClip(RandomAudioClip, position);
        public AudioAccess PlayRandomAudioClipOnLoop(Transform parent) => PlayAudioClipOnLoop(RandomAudioClip, parent);

        public AudioAccess Play(Vector3 position) => PlayAudioClip(NonRepeatingRandomAudioClip, position);
        public AudioAccess PlayNonRepeatingAudioClipOnLoop(Transform parent) => PlayAudioClipOnLoop(NonRepeatingRandomAudioClip, parent);

        public AudioAccess PlayNextAudioClip(Vector3 position) => PlayAudioClip(NextAudioClip, position);
        public AudioAccess PlayNextAudioClipOnLoop(Transform parent) => PlayAudioClipOnLoop(NextAudioClip, parent);

        public AudioAccess Play() => PlayAudioClip(NonRepeatingRandomAudioClip, Vector3.zero);

        private AudioAccess PlayAudioClip(AudioClip clip, Vector3 position)
        {
            AudioProperties properties = new(clip, OutputAudioGroup, RandomVolume, Priority, RandomSpatialBlend, RandomPitch, false, null, position);

            return s_audioPlayer.Access.Play(properties);
        }

        private AudioAccess PlayAudioClipOnLoop(AudioClip clip, Transform parent)
        {
            AudioProperties properties = new(clip, OutputAudioGroup, RandomVolume, Priority, RandomSpatialBlend, RandomPitch, true, parent, Vector3.zero);

            return s_audioPlayer.Access.Play(properties);
        }

        private void ReplenishNonRepeatingAudioClipsIndices()
        {
            var indices = AuxMath.GetRandomNumbersWithoutRepetition(0, _audioClips.Count, _audioClips.Count);
            var enumerator = indices.GetEnumerator();

            while (enumerator.MoveNext() == true) _nonRepeatingAudioClipsIndices.Enqueue(enumerator.Current);
        }

        [Button("Audio preview")]
        private void AudioPreview()
        {
            if (AudioClipsAmount == 0) return;

            GameObject previewObject = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideInHierarchy);
            var previewSource = previewObject.AddComponent<AudioSource>();

            AudioClip randomClip = RandomAudioClip;
            float pitch = RandomPitch;

            previewSource.clip = randomClip;
            previewSource.spatialBlend = RandomSpatialBlend;
            previewSource.pitch = RandomPitch;
            previewSource.volume = RandomVolume;
            previewSource.outputAudioMixerGroup = OutputAudioGroup;

            previewSource.Play();

            ClearAudioPreviewObject(previewObject, randomClip.length * pitch).Forget();
        }

        private async UniTaskVoid ClearAudioPreviewObject(GameObject previewObject, float delay)
        {
            await UniTask.WaitForSeconds(delay);

            DestroyImmediate(previewObject);
        }
    }
}