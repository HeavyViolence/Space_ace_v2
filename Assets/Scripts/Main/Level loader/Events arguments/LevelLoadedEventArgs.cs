using System;

namespace SpaceAce.Main
{
    public sealed class LevelLoadedEventArgs : EventArgs
    {
        public int LevelIndex { get; }

        public LevelLoadedEventArgs(int levelIndex)
        {
            LevelIndex = levelIndex;
        }
    }
}