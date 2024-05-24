using System;

namespace SpaceAce.Main
{
    public sealed class LevelLoadingStartedEventArgs : EventArgs
    {
        public int Level { get; }
        public float LoadingDelay { get; }

        public LevelLoadingStartedEventArgs(int index, float loadingDelay)
        {
            if (index < 1) throw new ArgumentOutOfRangeException();
            Level = index;

            if (loadingDelay <= 0f) throw new ArgumentOutOfRangeException();
            LoadingDelay = loadingDelay;
        }
    }
}