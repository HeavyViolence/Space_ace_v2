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
        public int LoadedLevel { get; private set; } = 0;

        public async UniTask LoadMainMenuAsync(float delay)
        {
            if (CurrentState == GameState.MainMenu ||
                CurrentState == GameState.MainMenuLoading) return;

            float clampedDelay = Mathf.Clamp(delay, MinLoadingDelay, MaxLoadingDelay);

            MainMenuLoadingStarted?.Invoke(this, new(clampedDelay));
            CurrentState = GameState.MainMenuLoading;

            await UniTask.WaitForSeconds(clampedDelay);

            MainMenuLoaded?.Invoke(this, EventArgs.Empty);
            CurrentState = GameState.MainMenu;
            LoadedLevel = 0;
        }

        public async UniTask LoadLevelAsync(int level, float delay)
        {
            if (level <= 0) throw new ArgumentOutOfRangeException();
            if (LoadedLevel == level) return;

            float clampedDelay = Mathf.Clamp(delay, MinLoadingDelay, MaxLoadingDelay);

            LevelLoadingStarted?.Invoke(this, new(level, clampedDelay));
            CurrentState = GameState.LevelLoading;

            await UniTask.WaitForSeconds(clampedDelay);

            LevelLoaded?.Invoke(this, new(level));
            CurrentState = GameState.Level;
            LoadedLevel = level;
        }
    }
}