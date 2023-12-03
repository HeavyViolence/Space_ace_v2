using Cysharp.Threading.Tasks;

using NaughtyAttributes;

using SpaceAce.Auxiliary;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.Audio;

namespace SpaceAce.Main.Audio
{
    [CreateAssetMenu(fileName = "Audio collection",
                     menuName = "Space ace/Configs/Audio/Audio collection")]
    public class AudioCollection : ScriptableObject
    {
        public const float MinSpatialBlend = 0f;
        public const float MaxSpatialBlend = 1f;
        public const float DefaultSpatialBlend = 0.5f;

        public const float MinPitch = 0.5f;
        public const float MaxPitch = 2f;
        public const float DefaultPitch = 1f;

        private int _nextAudioClipIndex = 0;
        private Queue<int> _nonRepeatingAudioClipsIndices;

        [SerializeField]
        private List<AudioClip> _audioClips;

        [SerializeField]
        private AudioMixerGroup _outputAudioGroup;

        [SerializeField, MinMaxSlider(0f, 1f)]
        private Vector2 _volume = new(1f, 1f);

        [SerializeField]
        private AudioPriority _priority = AudioPriority.Lowest;

        [SerializeField, MinMaxSlider(MinSpatialBlend, MaxSpatialBlend)]
        private Vector2 _spatialBlend = new(0f, 0f);

        [SerializeField, MinMaxSlider(MinPitch, MaxPitch)]
        private Vector2 _pitch = new(MinPitch, MaxPitch);

        public int AudioClipsAmount => _audioClips.Count;

        public AudioProperties Next => new(NextAudioClip,
                                           _outputAudioGroup,
                                           RandomVolume,
                                           _priority,
                                           RandomSpatialBlend,
                                           RandomPitch);

        public AudioProperties Random => new(RandomAudioClip,
                                             _outputAudioGroup,
                                             RandomVolume,
                                             _priority,
                                             RandomSpatialBlend,
                                             RandomPitch);

        public AudioProperties NonRepeatingRandom => new(NonRepeatingRandomAudioClip,
                                                         _outputAudioGroup,
                                                         RandomVolume,
                                                         _priority,
                                                         RandomSpatialBlend,
                                                         RandomPitch);

        private float RandomVolume => AuxMath.GetRandom(_volume.x, _volume.y);

        private float RandomSpatialBlend => AuxMath.GetRandom(_spatialBlend.x, _spatialBlend.y);

        private float RandomPitch => AuxMath.GetRandom(_pitch.x, _pitch.y);

        private AudioClip RandomAudioClip => _audioClips[AuxMath.GetRandom(0, _audioClips.Count)];

        private AudioClip NonRepeatingRandomAudioClip
        {
            get
            {
                if (_nonRepeatingAudioClipsIndices is null ||
                    _nonRepeatingAudioClipsIndices.Count == 0)
                    ReplenishNonRepeatingAudioClipsIndices();

                int index = _nonRepeatingAudioClipsIndices.Dequeue();
                return _audioClips[index];
            }
        }

        private AudioClip NextAudioClip => _audioClips[_nextAudioClipIndex++ % _audioClips.Count];

        private void ReplenishNonRepeatingAudioClipsIndices()
        {
            var indices = AuxMath.GetRandomNumbersWithoutRepetition(0, _audioClips.Count, _audioClips.Count);
            _nonRepeatingAudioClipsIndices = new(indices);
        }

        [Button("Audio preview")]
        private async UniTaskVoid AudioPreview()
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
            previewSource.outputAudioMixerGroup = _outputAudioGroup;

            previewSource.Play();

            await UniTask.WaitForSeconds(randomClip.length * pitch);

            DestroyImmediate(previewObject);
        }
    }
}