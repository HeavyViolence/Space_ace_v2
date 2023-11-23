namespace SpaceAce.Gameplay.Levels
{
    public interface ILevelUnlocker
    {
        public int LargestCompletedLevelIndex { get; }
        public int LargestUnlockedLevelIndex { get; }
    }
}