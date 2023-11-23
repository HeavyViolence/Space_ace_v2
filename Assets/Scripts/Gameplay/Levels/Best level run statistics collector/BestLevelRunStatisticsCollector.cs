using Newtonsoft.Json;

using SpaceAce.Gameplay.Players;
using SpaceAce.Main.Saving;

using System;
using System.Collections.Generic;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class BestLevelRunStatisticsCollector : IBestLevelRunStatisticsCollector, IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly Dictionary<int, BestLevelRunStatistics> _statistics = new();

        private readonly ISavingSystem _savingSystem;
        private readonly ILevelCompleter _levelCompleter;
        private readonly ILevelStopwatch _levelStopwatch;
        private readonly IPlayer _player;

        public string ID => "Best levels runs statistics";

        public BestLevelRunStatisticsCollector(ISavingSystem savingSystem,
                                               ILevelCompleter levelCompleter,
                                               ILevelStopwatch levelStopwatch,
                                               IPlayer player)
        {
            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");

            _levelCompleter = levelCompleter ?? throw new ArgumentNullException(nameof(levelCompleter),
                $"Attempted to pass an empty {typeof(ILevelCompleter)}!");

            _levelStopwatch = levelStopwatch ?? throw new ArgumentNullException(nameof(levelStopwatch),
                $"Attempted to pass an empty {typeof(ILevelStopwatch)}!");

            _player = player ?? throw new ArgumentNullException(nameof(player),
                $"Attempted to pass an mepty {typeof(IPlayer)}!");
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