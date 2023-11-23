using Cysharp.Threading.Tasks;

using System;

namespace SpaceAce.Main
{
    public interface IGameStateLoader
    {
        const float MinLoadingDelay = 0.5f;
        const float MaxLoadingDelay = 2f;

        event EventHandler<MainMenuLoadingStartedEventArgs> MainMenuLoadingStarted;
        event EventHandler MainMenuLoaded;

        event EventHandler<LevelLoadingStartedEventArgs> LevelLoadingStarted;
        event EventHandler<LevelLoadedEventArgs> LevelLoaded;

        GameState CurrentState { get; }
        int LoadedLevelIndex { get; }

        UniTask LoadMainMenuAsync(float delay);
        UniTask LoadLevelAsync(int levelIndex, float delay);
    }
}