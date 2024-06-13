using System;

namespace SpaceAce.UI.Displays
{
    public sealed class LevelButtonCheckedEventArgs : EventArgs
    {
        public int Level { get; }

        public LevelButtonCheckedEventArgs(int level)
        {
            Level = level;
        }
    }
}