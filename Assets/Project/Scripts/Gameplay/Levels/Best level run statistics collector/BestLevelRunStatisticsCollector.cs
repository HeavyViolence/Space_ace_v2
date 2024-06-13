using Newtonsoft.Json;

using SpaceAce.Gameplay.Players;
using SpaceAce.Main.Saving;

using System;
using System.Collections.Generic;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class BestLevelRunStatisticsCollector : IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly Dictionary<int, BestLevelRunStatistics> _statistics = new();

        private readonly ISavingSystem _savingSystem;
        private readonly LevelCompleter _levelCompleter;
        private readonly LevelStopwatch _levelStopwatch;
        private readonly Player _player;

        public string SavedDataName => "Best levels runs statistics";

        public BestLevelRunStatisticsCollector(ISavingSystem savingSystem,
                                               LevelCompleter levelCompleter,
                                               LevelStopwatch levelStopwatch,
                                               Player player)
        {
            _savingSystem = savingSystem ?? throw new ArgumentNullException();
            _levelCompleter = levelCompleter ?? throw new ArgumentNullException();
            _levelStopwatch = levelStopwatch ?? throw new ArgumentNullException();
            _player = player ?? throw new ArgumentNullException();
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
            _levelCompleter.LevelCompleted += LevelCompletedEventHandler;
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);
            _levelCompleter.LevelCompleted -= LevelCompletedEventHandler;
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && SavedDataName == other.SavedDataName;

        public override int GetHashCode() => SavedDataName.GetHashCode();

        public string GetState() => JsonConvert.SerializeObject(_statistics);

        public void SetState(string state)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<int, BestLevelRunStatistics>>>(state);
                foreach (var element in data) _statistics.TryAdd(element.Key, element.Value);
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