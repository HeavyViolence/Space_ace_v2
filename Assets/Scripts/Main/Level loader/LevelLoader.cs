using Cysharp.Threading.Tasks;
using System;

namespace SpaceAce.Main
{
    public sealed class LevelLoader
    {
        private const float LevelLoadingDelay = 2f;

        public event EventHandler<LevelLoadingStartedEventArgs> LevelLoadingStarted;
        public event EventHandler<LevelLoadedEventArgs> LevelLoaded;

        public int LoadedLevel { get; private set; } = 0;

        public LevelLoader() { }

        public async UniTaskVoid LoadLevel(int levelIndex)
        {
            LevelLoadingStarted?.Invoke(this, new(levelIndex, LevelLoadingDelay));

            await UniTask.Delay(TimeSpan.FromSeconds(LevelLoadingDelay));

            LoadedLevel = levelIndex;
            LevelLoaded?.Invoke(this, new(levelIndex));
        }
    }
}
