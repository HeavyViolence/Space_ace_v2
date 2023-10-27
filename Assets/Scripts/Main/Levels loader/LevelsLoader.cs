using Cysharp.Threading.Tasks;
using System;

namespace SpaceAce.Main
{
    public sealed class LevelsLoader
    {
        private const float LevelLoadingDelay = 1f;

        public event EventHandler<LevelLoadingStartedEventArgs> LevelLoadingStarted;
        public event EventHandler<LevelLoadedEventArgs> LevelLoaded;

        public int LoadedLevel { get; private set; } = 0;

        public LevelsLoader() { }

        public async UniTask LoadLevelAsync(int levelIndex)
        {
            if (levelIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(levelIndex),
                    $"Level index must be greater than 0!");

            if (LoadedLevel == levelIndex) return;

            LevelLoadingStarted?.Invoke(this, new(levelIndex, LevelLoadingDelay));

            await UniTask.Delay(TimeSpan.FromSeconds(LevelLoadingDelay));

            LoadedLevel = levelIndex;
            LevelLoaded?.Invoke(this, new(levelIndex));
        }
    }
}