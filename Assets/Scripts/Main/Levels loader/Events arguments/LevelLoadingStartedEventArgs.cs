using System;

namespace SpaceAce.Main
{
    public sealed class LevelLoadingStartedEventArgs : EventArgs
    {
        public int LevelIndex { get; }
        public float LoadingDelay { get; }

        public LevelLoadingStartedEventArgs(int levelIndex, float loadingDelay)
        {
            if (levelIndex < 1) throw new ArgumentOutOfRangeException(nameof(levelIndex),
                "Level index must be greater than 0!");

            if (loadingDelay <= 0f) throw new ArgumentOutOfRangeException(nameof(loadingDelay),
                "Loading delay must be greater than 0!");

            LevelIndex = levelIndex;
            LoadingDelay = loadingDelay;
        }
    }
}