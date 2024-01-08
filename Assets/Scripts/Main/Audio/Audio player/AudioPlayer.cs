using System.Collections.Generic;
using System.Threading;
using System;

using UnityEngine.Audio;
using UnityEngine;

using Cysharp.Threading.Tasks;

using SpaceAce.Main.Saving;

using Newtonsoft.Json;

using Zenject;

namespace SpaceAce.Main.Audio
{
    public sealed class AudioPlayer : IInitializable, IDisposable, ISavable
    {
        private const int MaxAudioSources = 32;

        public event EventHandler SavingRequested;

        private readonly AudioMixer _audioMixer;
        private readonly ISavingSystem _savingSystem;
        private readonly GamePauser _gamePauser;

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
                if (value is null) throw new ArgumentNullException();

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

        public AudioPlayer(AudioMixer mixer, ISavingSystem savingSystem, GamePauser gamePauser)
        {
            if (mixer == null) throw new ArgumentNullException();
            _audioMixer = mixer;

            _savingSystem = savingSystem ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();

            CreateAudioSourcePool();
        }

        public async UniTask PlayOnceAsync(AudioProperties properties,
                                           Vector3 position,
                                           Transform anchor = null,
                                           CancellationToken token = default,
                                           bool obeyGamePause = false)
        {
            AudioSource source = FindAvailableAudioSource();
            AudioAccess access = ConfigureAudioSource(source, properties, position, anchor, false);

            float timer = 0f;

            while (timer < access.PlaybackDuration)
            {
                timer += Time.deltaTime;

                if (token != default &&
                    token.IsCancellationRequested == true) break;

                if (obeyGamePause == true && _gamePauser.Paused == true)
                {
                    source.Pause();

                    while (_gamePauser.Paused == true) await UniTask.Yield();

                    source.Play();
                }

                await UniTask.Yield();
            }

            DisableActiveAudioSource(access.ID);
        }

        public async UniTask PlayInLoopAsync(AudioProperties properties,
                                             Vector3 position,
                                             Transform anchor = null,
                                             CancellationToken token = default,
                                             bool obeyGamePause = false)
        {
            AudioSource source = FindAvailableAudioSource();
            AudioAccess access = ConfigureAudioSource(source, properties, position, anchor, true);

            float timer = 0f;

            if (token == default)
            {
                while (timer < access.PlaybackDuration)
                {
                    timer += Time.deltaTime;

                    if (obeyGamePause == true && _gamePauser.Paused == true)
                    {
                        source.Pause();

                        while (_gamePauser.Paused == true) await UniTask.Yield();

                        source.Play();
                    }

                    await UniTask.Yield();
                }
            }
            else
            {
                while (token.IsCancellationRequested == false)
                {
                    if (obeyGamePause == true && _gamePauser.Paused == true)
                    {
                        source.Pause();

                        while (_gamePauser.Paused == true) await UniTask.Yield();

                        source.Play();
                    }

                    await UniTask.Yield();
                }
            }

            DisableActiveAudioSource(access.ID);
        }

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
            if (source == null) return;

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

        private AudioAccess ConfigureAudioSource(AudioSource source,
                                                 AudioProperties properties,
                                                 Vector3 position,
                                                 Transform anchor,
                                                 bool loop)
        {
            if (properties is null)
                throw new ArgumentNullException(nameof(properties),
                    $"Attempted to pass an empty {typeof(AudioProperties)}!");

            AudioAccess access;
            var id = Guid.NewGuid();

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

            if (anchor != null) source.transform.parent = anchor;

            if (loop == true)
            {
                source.loop = true;
                source.transform.localPosition = position;

                access = new AudioAccess(id, float.PositiveInfinity);
            }
            else
            {
                source.loop = false;
                source.transform.position = position;

                access = new AudioAccess(id, properties.Clip.length / properties.Pitch);
            }

            source.gameObject.SetActive(true);
            source.Play();

            _activeAudioSources.Add(id, source);

            return access;
        }

        private AudioSource FindAvailableAudioSource()
        {
            if (_availableAudioSources.Count > 0)
                return _availableAudioSources.Pop();

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

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.ID == ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion
    }
}