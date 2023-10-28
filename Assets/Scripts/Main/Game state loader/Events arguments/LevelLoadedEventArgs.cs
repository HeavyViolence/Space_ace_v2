using System;

namespace SpaceAce.Main
{
    public sealed class LevelLoadedEventArgs : EventArgs
    {
        public int LevelIndex { get; }

        public LevelLoadedEventArgs(int levelIndex)
        {
            if (levelIndex < 1)
                throw new ArgumentOutOfRangeException(nameof(levelIndex),
                    "Level index must be greater than 0!");

            LevelIndex = levelIndex;
        }
    }
}