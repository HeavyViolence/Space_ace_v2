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
        private const int MaxAudioSources = 64;

        public event EventHandler SavingRequested;

        private readonly AudioMixer _audioMixer;
        private readonly ISavingSystem _savingSystem;
        private readonly GamePauser _gamePauser;

        private readonly Transform _audioSourcesAnchor = new GameObject("Audio sources anchor").transform;
        private readonly Dictionary<Guid, AudioSourceCache> _activeAudioSources = new(MaxAudioSources);
        private readonly Stack<AudioSourceCache> _availableAudioSources = new(MaxAudioSources);

        public string SavedDataName => "Audio settings";

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
                                           bool obeyGamePause = false,
                                           CancellationToken token = default)
        {
            AudioSourceCache cache = FindAvailableAudioSource();
            AudioAccess access = ConfigureAudioSource(cache, properties, position, anchor, false);

            float timer = 0f;

            while (timer < access.PlaybackDuration)
            {
                timer += Time.deltaTime;

                if (token.IsCancellationRequested == true) break;

                if (obeyGamePause == true && _gamePauser.Paused == true)
                {
                    cache.AudioSource.Pause();

                    while (_gamePauser.Paused == true) await UniTask.Yield();

                    cache.AudioSource.Play();
                }

                await UniTask.Yield();
            }

            DisableActiveAudioSource(access.ID);
        }

        public async UniTask PlayInLoopAsync(AudioProperties properties,
                                             Vector3 position,
                                             Transform anchor = null,
                                             bool obeyGamePause = false,
                                             CancellationToken token = default)
        {
            AudioSourceCache cache = FindAvailableAudioSource();
            AudioAccess access = ConfigureAudioSource(cache, properties, position, anchor, true);

            if (token == default)
            {
                float timer = 0f;

                while (timer < properties.Clip.length)
                {
                    timer += Time.deltaTime;

                    if (obeyGamePause == true && _gamePauser.Paused == true)
                    {
                        cache.AudioSource.Pause();

                        while (_gamePauser.Paused == true) await UniTask.Yield();

                        cache.AudioSource.Play();
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
                        cache.AudioSource.Pause();

                        while (_gamePauser.Paused == true) await UniTask.Yield();

                        cache.AudioSource.Play();
                    }

                    await UniTask.Yield();
                }
            }

            DisableActiveAudioSource(access.ID);
        }

        private void CreateAudioSourcePool()
        {
            for (int i = 0; i < MaxAudioSources; i++)
            {
                var audioSourceHolder = new GameObject($"Audio source #{i + 1}");
                audioSourceHolder.transform.parent = _audioSourcesAnchor;
                var audioSource = audioSourceHolder.AddComponent<AudioSource>();
                AudioSourceCache cache = new(audioSource, audioSourceHolder.transform);

                ResetAudioSource(cache);
                _availableAudioSources.Push(cache);
            }
        }

        private bool DisableActiveAudioSource(Guid id)
        {
            if (_activeAudioSources.TryGetValue(id, out AudioSourceCache cache) == true)
            {
                ResetAudioSource(cache);

                _activeAudioSources.Remove(id);
                _availableAudioSources.Push(cache);

                return true;
            }

            return false;
        }

        private void ResetAudioSource(AudioSourceCache cache)
        {
            if (cache is null || cache.AudioSource == null || cache.Transform == null) return;

            cache.AudioSource.Stop();

            cache.AudioSource.clip = null;
            cache.AudioSource.outputAudioMixerGroup = null;
            cache.AudioSource.mute = true;
            cache.AudioSource.bypassEffects = true;
            cache.AudioSource.bypassListenerEffects = true;
            cache.AudioSource.bypassReverbZones = true;
            cache.AudioSource.playOnAwake = false;
            cache.AudioSource.loop = false;
            cache.AudioSource.priority = byte.MaxValue;
            cache.AudioSource.volume = 0f;
            cache.AudioSource.spatialBlend = 0f;
            cache.AudioSource.pitch = 1f;
            cache.AudioSource.reverbZoneMix = 0f;

            cache.Transform.parent = _audioSourcesAnchor.transform;
            cache.Transform.position = Vector3.zero;
            cache.Transform.gameObject.SetActive(false);
        }

        private AudioAccess ConfigureAudioSource(AudioSourceCache cache,
                                                 AudioProperties properties,
                                                 Vector3 position,
                                                 Transform anchor,
                                                 bool loop)
        {
            AudioAccess access;
            var id = Guid.NewGuid();

            cache.AudioSource.clip = properties.Clip;
            cache.AudioSource.outputAudioMixerGroup = properties.OutputAudioGroup;
            cache.AudioSource.mute = false;
            cache.AudioSource.bypassEffects = false;
            cache.AudioSource.bypassListenerEffects = false;
            cache.AudioSource.bypassReverbZones = false;
            cache.AudioSource.priority = (int)properties.Priority;
            cache.AudioSource.volume = properties.Volume;
            cache.AudioSource.spatialBlend = properties.SpatialBlend;
            cache.AudioSource.pitch = properties.Pitch;

            if (anchor != null) cache.Transform.parent = anchor;

            if (loop == true)
            {
                cache.AudioSource.loop = true;
                cache.Transform.localPosition = position;

                access = new AudioAccess(id, float.PositiveInfinity);
            }
            else
            {
                cache.AudioSource.loop = false;
                cache.Transform.position = position;

                access = new AudioAccess(id, properties.Clip.length);
            }

            cache.Transform.gameObject.SetActive(true);
            cache.AudioSource.Play();

            _activeAudioSources.Add(id, cache);

            return access;
        }

        private AudioSourceCache FindAvailableAudioSource()
        {
            if (_availableAudioSources.Count > 0)
                return _availableAudioSources.Pop();

            byte priority = 0;
            Guid id = Guid.Empty;
            AudioSourceCache availableSource = null;

            foreach (var entry in _activeAudioSources)
            {
                if (entry.Value.AudioSource.loop == true) continue;

                if (entry.Value.AudioSource.priority > priority)
                {
                    priority = (byte)entry.Value.AudioSource.priority;
                    id = entry.Key;
                    availableSource = entry.Value;
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

        public bool Equals(ISavable other) => other is not null && other.SavedDataName == SavedDataName;

        public override int GetHashCode() => SavedDataName.GetHashCode();

        #endregion
    }
}