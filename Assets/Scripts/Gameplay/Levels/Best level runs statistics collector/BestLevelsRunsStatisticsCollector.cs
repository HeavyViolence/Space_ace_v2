using Newtonsoft.Json;
using SpaceAce.Architecture;
using SpaceAce.Gameplay.Players;
using SpaceAce.Main.Saving;
using System;
using System.Collections.Generic;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class BestLevelsRunsStatisticsCollector : IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly Dictionary<int, BestLevelRunStatistics> _statistics = new();
        private readonly ISavingSystem _savingSystem = null;
        private readonly LevelsCompleter _levelsCompleter = null;
        private readonly LevelStopwatch _levelStopwatch = null;
        private readonly Player _player = null;

        public string ID => "Best levels runs statistics";

        public BestLevelsRunsStatisticsCollector(ISavingSystem savingSystem,
                                                 LevelsCompleter levelsCompleter,
                                                 LevelStopwatch levelStopWatch,
                                                 Player player)
        {
            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");

            _levelsCompleter = levelsCompleter ?? throw new ArgumentNullException(nameof(levelsCompleter),
                $"Attempted to pass an empty {typeof(LevelsCompleter)}!");

            _levelStopwatch = levelStopWatch ?? throw new ArgumentNullException(nameof(levelStopWatch),
                $"Attempted to pass an empty {typeof(LevelStopwatch)}!");

            _player = player ?? throw new ArgumentNullException(nameof(player),
                $"Attempted to pass an empty {typeof(Player)}!");
        }

        public BestLevelRunStatistics GetStatistics(int levelIndex)
        {
            if (_statistics.TryGetValue(levelIndex, out var value) == true) return value;

            return BestLevelRunStatistics.Default;
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);
            _levelsCompleter.LevelCompleted += LevelCompletedEventHandler;
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);
            _levelsCompleter.LevelCompleted -= LevelCompletedEventHandler;
        }

        public override bool Equals(object obj) => Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && ID == other.ID;

        public override int GetHashCode() => ID.GetHashCode();

        public string GetState() => JsonConvert.SerializeObject(_statistics);

        public void SetState(string state)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<int, BestLevelRunStatistics>>>(state);

                foreach (var element in data)
                    _statistics.TryAdd(element.Key, element.Value);
            }
            catch (Exception) { }
        }

        #endregion

        #region event handlers

        private void LevelCompletedEventHandler(object sender, LevelEndedEventArgs e)
        {
            SavingRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}