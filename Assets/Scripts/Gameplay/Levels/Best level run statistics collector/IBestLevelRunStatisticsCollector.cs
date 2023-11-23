namespace SpaceAce.Gameplay.Levels
{
    public interface IBestLevelRunStatisticsCollector
    {
        BestLevelRunStatistics GetStatistics(int levelIndex);
    }
}