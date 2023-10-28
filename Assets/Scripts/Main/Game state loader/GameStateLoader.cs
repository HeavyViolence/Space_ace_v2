using Cysharp.Threading.Tasks;

using System;

namespace SpaceAce.Main
{
    public sealed class GameStateLoader
    {
        private const float LoadingDelay = 1f;

        public event EventHandler<MainMenuLoadingStartedEventArgs> MainMenuLoadingStarted;
        public event EventHandler MainMenuLoaded;

        public event EventHandler<LevelLoadingStartedEventArgs> LevelLoadingStarted;
        public event EventHandler<LevelLoadedEventArgs> LevelLoaded;

        public GameState GameState { get; private set; } = GameState.MainMenu;
        public int LoadedLevelIndex { get; private set; } = 0;

        public async UniTask LoadMainMenuAsync()
        {
            if (GameState == GameState.MainMenu ||
                GameState == GameState.MainMenuLoading) return;

            MainMenuLoadingStarted?.Invoke(this, new(LoadingDelay));
            GameState = GameState.MainMenuLoading;

            await UniTask.Delay(TimeSpan.FromSeconds(LoadingDelay));

            MainMenuLoaded?.Invoke(this, EventArgs.Empty);
            GameState = GameState.MainMenu;
            LoadedLevelIndex = 0;
        }

        public async UniTask LoadLevelAsync(int levelIndex)
        {
            if (levelIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(levelIndex),
                    $"Level index must be greater than 0!");

            if (LoadedLevelIndex == levelIndex) return;

            LevelLoadingStarted?.Invoke(this, new(levelIndex, LoadingDelay));
            GameState = GameState.LevelLoading;

            await UniTask.Delay(TimeSpan.FromSeconds(LoadingDelay));

            LevelLoaded?.Invoke(this, new(levelIndex));
            GameState = GameState.Level;
            LoadedLevelIndex = levelIndex;
        }
    }
}