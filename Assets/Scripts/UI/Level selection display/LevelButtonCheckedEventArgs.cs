using System;

namespace SpaceAce.UI
{
    public sealed class LevelButtonCheckedEventArgs : EventArgs
    {
        public int LevelIndex { get; }

        public LevelButtonCheckedEventArgs(int levelIndex)
        {
            LevelIndex = levelIndex;
        }
    }
}