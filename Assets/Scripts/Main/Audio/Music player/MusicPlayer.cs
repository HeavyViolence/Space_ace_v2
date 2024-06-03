using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using SpaceAce.Main.Saving;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Audio
{
    public sealed class MusicPlayer : IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly AudioCollection _music;
        private readonly AudioPlayer _audioPlayer;
        private readonly ISavingSystem _savingSystem;

        private CancellationTokenSource _musicCancellation;

        public bool IsPlaying => _musicCancellation is not null;

        private MusicPlayerSettings _settings = MusicPlayerSettings.Default;
        public MusicPlayerSettings Settings
        {
            get => _settings;

            set
            {
                _settings = value ?? throw new ArgumentNullException();
                SavingRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public string SavedDataName => "Music settings";


        public MusicPlayer(AudioCollection music, AudioPlayer audioPlayer, ISavingSystem savingSystem)
        {
            if (music == null) throw new ArgumentNullException();
            _music = music;

            _audioPlayer = audioPlayer ?? throw new ArgumentNullException();
            _savingSystem = savingSystem ?? throw new ArgumentNullException();
        }

        public void Play()
        {
            if (IsPlaying == true) return;

            _musicCancellation = new();
            PlayMusicForeverAsync(_musicCancellation.Token).Forget();
        }

        public void Stop()
        {
            if (IsPlaying == false) return;

            _musicCancellation?.Cancel();
            _musicCancellation?.Dispose();
            _musicCancellation = null;
        }

        private async UniTaskVoid PlayMusicForeverAsync(CancellationToken token)
        {
            await UniTask.WaitForSeconds(Settings.PlaybackStartDelay, true, PlayerLoopTiming.Update, token);

            while (token.IsCancellationRequested == false)
            {
                await _audioPlayer.PlayOnceAsync(_music.NonRepeatingRandom, Vector3.zero, null, true, token);
                await UniTask.WaitForSeconds(Settings.PlaybackDelay, true, PlayerLoopTiming.Update, token);
            }
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);
            if (Settings.Autoplay == true) Play();
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);
            Stop();
        }

        public string GetState() => JsonConvert.SerializeObject(Settings);

        public void SetState(string state)
        {
            try
            {
                _settings = JsonConvert.DeserializeObject<MusicPlayerSettings>(state);
            }
            catch (Exception)
            {
                _settings = MusicPlayerSettings.Default;
            }
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.SavedDataName == SavedDataName;

        public override int GetHashCode() => SavedDataName.GetHashCode();

        #endregion
    }
}