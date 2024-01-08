using Cysharp.Threading.Tasks;

using System;

using UnityEngine;

namespace SpaceAce.Main
{
    public sealed class GameStateLoader
    {
        public const float MinLoadingDelay = 0.5f;
        public const float MaxLoadingDelay = 2f;

        public event EventHandler<MainMenuLoadingStartedEventArgs> MainMenuLoadingStarted;
        public event EventHandler MainMenuLoaded;

        public event EventHandler<LevelLoadingStartedEventArgs> LevelLoadingStarted;
        public event EventHandler<LevelLoadedEventArgs> LevelLoaded;

        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public int LoadedLevelIndex { get; private set; } = 0;

        public async UniTask LoadMainMenuAsync(float delay)
        {
            if (CurrentState == GameState.MainMenu ||
                CurrentState == GameState.MainMenuLoading) return;

            float clampedDelay = Mathf.Clamp(delay, MinLoadingDelay, MaxLoadingDelay);

            MainMenuLoadingStarted?.Invoke(this, new(clampedDelay));
            CurrentState = GameState.MainMenuLoading;

            await UniTask.Delay(TimeSpan.FromSeconds(clampedDelay));

            MainMenuLoaded?.Invoke(this, EventArgs.Empty);
            CurrentState = GameState.MainMenu;
            LoadedLevelIndex = 0;
        }

        public async UniTask LoadLevelAsync(int levelIndex, float delay)
        {
            if (levelIndex <= 0) throw new ArgumentOutOfRangeException();
            if (LoadedLevelIndex == levelIndex) return;

            float clampedDelay = Mathf.Clamp(delay, MinLoadingDelay, MaxLoadingDelay);

            LevelLoadingStarted?.Invoke(this, new(levelIndex, clampedDelay));
            CurrentState = GameState.LevelLoading;

            await UniTask.Delay(TimeSpan.FromSeconds(clampedDelay));

            LevelLoaded?.Invoke(this, new(levelIndex));
            CurrentState = GameState.Level;
            LoadedLevelIndex = levelIndex;
        }
    }
}