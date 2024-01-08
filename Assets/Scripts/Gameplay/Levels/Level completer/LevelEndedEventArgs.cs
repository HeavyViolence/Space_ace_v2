using System;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelEndedEventArgs : EventArgs
    {
        public int LevelIndex { get; }

        public LevelEndedEventArgs(int levelIndex)
        {  
            if (levelIndex < 1) throw new ArgumentOutOfRangeException();
            LevelIndex = levelIndex;
        }
    }
}