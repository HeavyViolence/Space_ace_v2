using System;

namespace SpaceAce.Main
{
    public sealed class LevelLoadingStartedEventArgs : EventArgs
    {
        public int LevelIndex { get; }
        public float LoadingDelay { get; }

        public LevelLoadingStartedEventArgs(int levelIndex, float loadingDelay)
        {
            LevelIndex = levelIndex;
            LoadingDelay = loadingDelay;
        }
    }
}