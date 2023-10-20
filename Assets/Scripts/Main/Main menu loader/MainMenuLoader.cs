using Cysharp.Threading.Tasks;
using SpaceAce.Architecture;
using System;

namespace SpaceAce.Main
{
    public sealed class MainMenuLoader : IInitializable, IDisposable
    {
        private const float MainMenuLoadingDelay = 1f;

        public event EventHandler<MainMenuLoadingStartedEventArgs> MainMenuLoadingStarted;
        public event EventHandler MainMenuLoaded;

        private readonly LevelsLoader _levelsLoader = null;

        public bool IsMainMenuLoaded { get; private set; } = true;

        public MainMenuLoader(LevelsLoader levelsLoader)
        {
            _levelsLoader = levelsLoader ?? throw new ArgumentNullException(nameof(levelsLoader),
                $"Attempted to pass an empty {typeof(LevelsLoader)}!");
        }

        public async UniTaskVoid LoadMainMenuAsync()
        {
            if (IsMainMenuLoaded) return;

            MainMenuLoadingStarted?.Invoke(this, new(MainMenuLoadingDelay));

            await UniTask.Delay(TimeSpan.FromSeconds(MainMenuLoadingDelay));

            MainMenuLoaded?.Invoke(this, EventArgs.Empty);
            IsMainMenuLoaded = true;
        }

        public void Initialize()
        {
            _levelsLoader.LevelLoaded += (sender, args) => IsMainMenuLoaded = false;
        }

        public void Dispose()
        {
            _levelsLoader.LevelLoaded -= (sender, args) => IsMainMenuLoaded = false;
        }
    }
}