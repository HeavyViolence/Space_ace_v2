using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SpaceAce.Architecture;
using SpaceAce.Main.Saving;
using System;
using System.Threading;

namespace SpaceAce.Main.Audio
{
    public sealed class MusicPlayer : IInitializable, IDisposable, IRunnable, IStoppable, ISavable
    {
        public event EventHandler SavingRequested;

        private CancellationTokenSource _musicCancellationTokenSource = null;
        private CancellationToken _musicCancellationToken;

        private readonly IMusic _music = null;
        private readonly ISavingSystem _savingSystem = null;

        private bool _isFirstTrackToPlay = true;

        public bool IsPlaying { get; private set; } = false;

        private MusicPlayerSettings _settings = MusicPlayerSettings.Default;
        public MusicPlayerSettings Settings
        {
            get => _settings;

            set
            {
                if (value is null) throw new ArgumentNullException(nameof(value),
                    $"Attempted to pass an empty {typeof(MusicPlayerSettings)}!");

                _settings = value;
                SavingRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ID => "Music settings";

        public MusicPlayer(IMusic music, ISavingSystem savingSystem)
        {
            if (music is null) throw new ArgumentNullException(nameof(music),
                $"Attempted to pass an empty {typeof(IMusic)}!");

            _music = music;

            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");
        }

        public void Play()
        {
            if (IsPlaying == true) return;

            IsPlaying = true;
            
            _musicCancellationTokenSource = new();
            _musicCancellationToken = _musicCancellationTokenSource.Token;

            PlayMusicForever(_musicCancellationToken).Forget();
        }

        public void Stop()
        {
            if (IsPlaying == false) return;

            IsPlaying = false;
            _isFirstTrackToPlay = true;

            _musicCancellationTokenSource.Cancel();
            _musicCancellationTokenSource.Dispose();
        }

        private async UniTaskVoid PlayMusicForever(CancellationToken token)
        {
            while (true)
            {
                if (_isFirstTrackToPlay)
                {
                    _isFirstTrackToPlay = false;
                    await UniTask.WaitForSeconds(Settings.PlaybackStartDelay, true, PlayerLoopTiming.Update, token);
                }

                AudioAccess trackAccess = _music.Play();
                float duration = trackAccess.PlaybackDuration + Settings.PlaybackDelay;

                await UniTask.WaitForSeconds(duration, true, PlayerLoopTiming.Update, token);

                if (token.IsCancellationRequested) break;
            }
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

        public void Run()
        {
            Play();
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