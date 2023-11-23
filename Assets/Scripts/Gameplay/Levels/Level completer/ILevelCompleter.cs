using System;

namespace SpaceAce.Gameplay.Levels
{
    public interface ILevelCompleter
    {
        event EventHandler<LevelEndedEventArgs> LevelCompleted;
        event EventHandler<LevelEndedEventArgs> LevelFailed;
        event EventHandler<LevelEndedEventArgs> LevelConcluded;
    }
}