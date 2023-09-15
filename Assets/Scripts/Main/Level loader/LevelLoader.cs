using Cysharp.Threading.Tasks;
using System;

namespace SpaceAce.Main
{
    public sealed class LevelLoader
    {
        private const float LevelLoadingDelay = 2f;
        private const float MainMenuLoadingDelay = 2f;

        public event EventHandler<LevelLoadingStartedEventArgs> LevelLoadingStarted;
        public event EventHandler<LevelLoadedEventArgs> LevelLoaded;

        public event EventHandler<MainMenuLoadingStartedEventArgs> MainMenuLoadingStarted;
        public event EventHandler MainMenuLoaded;

        public int LoadedLevel { get; private set; } = 0;

        public LevelLoader() { }

        public async UniTaskVoid LoadLevel(int levelIndex)
        {
            LevelLoadingStarted?.Invoke(this, new(levelIndex, LevelLoadingDelay));

            await UniTask.Delay(TimeSpan.FromSeconds(LevelLoadingDelay));

            LoadedLevel = levelIndex;
            LevelLoaded?.Invoke(this, new(levelIndex));
        }

        public async UniTaskVoid LoadMainMenu()
        {
            MainMenuLoadingStarted?.Invoke(this, new(MainMenuLoadingDelay));

            await UniTask.Delay(TimeSpan.FromSeconds(MainMenuLoadingDelay));

            LoadedLevel = 0;
            MainMenuLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
