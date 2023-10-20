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

        public async UniTaskVoid LoadLevelAsync(int levelIndex)
        {
            if (levelIndex <= 0) throw new ArgumentOutOfRangeException(nameof(levelIndex));

            LevelLoadingStarted?.Invoke(this, new(levelIndex, LevelLoadingDelay));

            await UniTask.Delay(TimeSpan.FromSeconds(LevelLoadingDelay));

            LoadedLevel = levelIndex;
            LevelLoaded?.Invoke(this, new(levelIndex));
        }
    }
}
