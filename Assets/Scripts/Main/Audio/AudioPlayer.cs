using System.Collections.Generic;
using System;
using UnityEngine.Audio;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SpaceAce.Main.Saving;
using Newtonsoft.Json;
using SpaceAce.Architecture;

namespace SpaceAce.Main.Audio
{
    public sealed class AudioPlayer : ISavable, IInitializable, IDisposable
    {
        private const int MaxAudioSources = 32;

        public event EventHandler SavingRequested;

        private readonly AudioMixer _audioMixer;
        private readonly ISavingSystem _savingSystem = null;

        private GameObject _audioSourcesAnchor;
        private readonly Dictionary<Guid, AudioSource> _activeAudioSources = new(MaxAudioSources);
        private readonly Stack<AudioSource> _availableAudioSources = new(MaxAudioSources);

        public string ID => "Audio settings";

        private AudioPlayerSettings _settings = AudioPlayerSettings.Default;
        public AudioPlayerSettings Settings
        {
            get => _settings;

            set
            {
                if (value is null) throw new ArgumentNullException(nameof(value), $"Attempted to pass an empty {typeof(AudioPlayerSettings)}!");

                _settings = value;

                _audioMixer.SetFloat("Master volume", value.MasterVolume);
                _audioMixer.SetFloat("Shooting volume", value.ShootingVolume);
                _audioMixer.SetFloat("Explosions volume", value.ExplosionsVolume);
                _audioMixer.SetFloat("Background volume", value.BackgroundVolume);
                _audioMixer.SetFloat("Interface volume", value.InterfaceVolume);
                _audioMixer.SetFloat("Music volume", value.MusicVolume);
                _audioMixer.SetFloat("Interactions volume", value.InteractionsVolume);
                _audioMixer.SetFloat("Notifications volume", value.NotificationsVolume);

                SavingRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public AudioPlayer(ISavingSystem savingSystem, AudioMixer mixer)
        {
            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");

            if (mixer == null)
                throw new ArgumentNullException(nameof(mixer),
                    $"Attempted to pass an empty {typeof(AudioMixer)}!");

            _audioMixer = mixer;

            CreateAudioSourcePool();
        }

        public AudioAccess Play(AudioProperties properties)
        {
            AudioSource availableSource = FindAvailableAudioSource();
            AudioAccess access = ConfigureAudioSource(availableSource, properties);

            return access;
        }

        public bool InterruptPlay(Guid id) => DisableActiveAudioSource(id);

        private void CreateAudioSourcePool()
        {
            _audioSourcesAnchor = new GameObject("Audio sources anchor");

            for (int i = 0; i < MaxAudioSources; i++)
            {
                var audioSourceHolder = new GameObject($"Audio source #{i + 1}");
                audioSourceHolder.transform.parent = _audioSourcesAnchor.transform;
                var audioSource = audioSourceHolder.AddComponent<AudioSource>();

                ResetAudioSource(audioSource);
                _availableAudioSources.Push(audioSource);
            }
        }

        private bool DisableActiveAudioSource(Guid id)
        {
            if (_activeAudioSources.TryGetValue(id, out AudioSource source) == true)
            {
                ResetAudioSource(source);
                _activeAudioSources.Remove(id);
                _availableAudioSources.Push(source);

                return true;
            }

            return false;
        }

        private void ResetAudioSource(AudioSource source)
        {
            source.Stop();

            source.clip = null;
            source.outputAudioMixerGroup = null;
            source.mute = true;
            source.bypassEffects = true;
            source.bypassListenerEffects = true;
            source.bypassReverbZones = true;
            source.playOnAwake = false;
            source.loop = false;
            source.priority = byte.MaxValue;
            source.volume = 0f;
            source.spatialBlend = 0f;
            source.pitch = 1f;
            source.reverbZoneMix = 0f;

            source.transform.parent = _audioSourcesAnchor.transform;
            source.transform.position = Vector3.zero;
            source.gameObject.SetActive(false);
        }

        private AudioAccess ConfigureAudioSource(AudioSource source, AudioProperties properties)
        {
            var id = Guid.NewGuid();
            AudioAccess access;

            source.clip = properties.Clip;
            source.outputAudioMixerGroup = properties.OutputAudioGroup;
            source.mute = false;
            source.bypassEffects = false;
            source.bypassListenerEffects = false;
            source.bypassReverbZones = false;
            source.priority = (byte)properties.Priority;
            source.volume = properties.Volume;
            source.spatialBlend = properties.SpatialBlend;
            source.pitch = properties.Pitch;

            if (properties.Loop == true)
            {
                source.loop = true;
                source.transform.parent = properties.AudioSourceAnchor;
                source.transform.localPosition = Vector3.zero;

                access = new AudioAccess(id, float.PositiveInfinity);
            }
            else
            {
                source.loop = false;
                source.transform.position = properties.PlayPosition;

                access = new AudioAccess(id, properties.Clip.length / properties.Pitch);

                WaitToDisableActiveAudioSource(access).Forget();
            }

            source.gameObject.SetActive(true);
            source.Play();

            _activeAudioSources.Add(id, source);

            return access;
        }

        private async UniTaskVoid WaitToDisableActiveAudioSource(AudioAccess access)
        {
            await UniTask.WaitForSeconds(access.PlaybackDuration);

            DisableActiveAudioSource(access.ID);
        }

        private AudioSource FindAvailableAudioSource()
        {
            if (_availableAudioSources.Count > 0) return _availableAudioSources.Pop();

            byte priority = 0;
            Guid id = Guid.Empty;
            AudioSource availableSource = null;

            foreach (var source in _activeAudioSources)
            {
                if (source.Value.loop == true) continue;

                if (source.Value.priority > priority)
                {
                    priority = (byte)source.Value.priority;
                    id = source.Key;
                    availableSource = source.Value;
                }
            }

            _activeAudioSources.Remove(id);
            ResetAudioSource(availableSource);

            return availableSource;
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);
        }

        public string GetState() => JsonConvert.SerializeObject(Settings);

        public void SetState(string state)
        {
            try
            {
                _settings = JsonConvert.DeserializeObject<AudioPlayerSettings>(state);
            }
            catch (Exception)
            {
                _settings = AudioPlayerSettings.Default;
            }
        }

        public override bool Equals(object other) => Equals(other as ISavable);

        public bool Equals(ISavable other) => other is not null && other.ID == ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion
    }
}