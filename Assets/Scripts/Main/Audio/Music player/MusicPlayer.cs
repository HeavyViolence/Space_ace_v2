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

        private CancellationTokenSource _musicCanceller;

        private readonly AudioCollection _music;
        private readonly AudioPlayer _audioPlayer;
        private readonly ISavingSystem _savingSystem;

        public bool IsPlaying { get; private set; } = false;

        private MusicPlayerSettings _settings = MusicPlayerSettings.Default;
        public MusicPlayerSettings Settings
        {
            get => _settings;

            set
            {
                _settings = value ?? throw new ArgumentNullException(nameof(value),
                        $"Attempted to pass an empty {typeof(MusicPlayerSettings)}!");

                SavingRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ID => "Music settings";


        public MusicPlayer(AudioCollection music,
                           AudioPlayer audioPlayer,
                           ISavingSystem savingSystem)
        {
            _music = music ?? throw new ArgumentNullException(nameof(music),
                    $"Attempted to pass an empty {typeof(AudioCollection)}!");

            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer),
                $"Attempted to pass an empty {typeof(AudioPlayer)}!");

            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");
        }

        public void Play()
        {
            if (IsPlaying == true) return;

            IsPlaying = true;

            _musicCanceller = new();
            PlayMusicForever(_musicCanceller.Token).Forget();
        }

        public void Stop()
        {
            if (IsPlaying == false) return;

            IsPlaying = false;

            _musicCanceller.Cancel();
            _musicCanceller.Dispose();
        }

        private async UniTaskVoid PlayMusicForever(CancellationToken token)
        {
            await UniTask.WaitForSeconds(Settings.PlaybackStartDelay, true, PlayerLoopTiming.Update, token);

            while (true)
            {
                await _audioPlayer.PlayOnceAsync(_music.NonRepeatingRandom, Vector3.zero, null, token);
                await UniTask.WaitForSeconds(Settings.PlaybackDelay, true, PlayerLoopTiming.Update, token);

                if (token.IsCancellationRequested == true) return;
            }
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);
            Play();
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

        public override bool Equals(object obj) => Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.ID == ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion
    }
}