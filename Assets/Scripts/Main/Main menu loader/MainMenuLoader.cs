using Cysharp.Threading.Tasks;
using SpaceAce.Architecture;
using System;

namespace SpaceAce.Main
{
    public sealed class MainMenuLoader : IInitializable, IDisposable
    {
        private const float MainMenuLoadingDelay = 2f;

        public event EventHandler<MainMenuLoadingStartedEventArgs> MainMenuLoadingStarted;
        public event EventHandler MainMenuLoaded;

        private readonly LevelLoader _levelLoader = null;

        public bool IsMainMenuLoaded { get; private set; } = true;

        public MainMenuLoader(LevelLoader levelLoader)
        {
            if (levelLoader is null) throw new ArgumentNullException($"Attempted to pass an empty {nameof(levelLoader)}!");
            _levelLoader = levelLoader;
        }

        public async UniTaskVoid LoadMainMenu()
        {
            if (IsMainMenuLoaded) return;

            MainMenuLoadingStarted?.Invoke(this, new(MainMenuLoadingDelay));

            await UniTask.Delay(TimeSpan.FromSeconds(MainMenuLoadingDelay));

            MainMenuLoaded?.Invoke(this, EventArgs.Empty);
            IsMainMenuLoaded = true;
        }

        public void Initialize()
        {
            _levelLoader.LevelLoaded += (sender, args) => IsMainMenuLoaded = false;
        }

        public void Dispose()
        {
            _levelLoader.LevelLoaded -= (sender, args) => IsMainMenuLoaded = false;
        }
    }
}