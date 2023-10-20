using Newtonsoft.Json;
using SpaceAce.Architecture;
using SpaceAce.Main.Saving;
using System;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelsUnlocker : IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly ISavingSystem _savingSystem = null;
        private readonly LevelsCompleter _levelsCompleter = null;

        public int HighestCompletedLevelIndex { get; private set; }
        public int HighestUnlockedLevelIndex => HighestCompletedLevelIndex + 1;

        public string ID => "Game progress";

        public LevelsUnlocker(ISavingSystem savingSystem , LevelsCompleter levelsCompleter)
        {
            if (savingSystem is null) throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");

            if (levelsCompleter is null) throw new ArgumentNullException(nameof(levelsCompleter),
                $"Attempted to pass an empty {typeof(LevelsCompleter)}!");

            _savingSystem = savingSystem;
            _levelsCompleter = levelsCompleter;
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

        public string GetState() => JsonConvert.SerializeObject(HighestCompletedLevelIndex);

        public void SetState(string state)
        {
            try
            {
                int index = JsonConvert.DeserializeObject<int>(state);
                HighestCompletedLevelIndex = index;
            }
            catch (Exception)
            {
                HighestCompletedLevelIndex = 0;
            }
        }

        public override bool Equals(object obj) => Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && ID == other.ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion

        #region event handlers

        private void LevelCompletedEventHandler(object sender, LevelEndedEventArgs e)
        {
            HighestCompletedLevelIndex = e.LevelIndex;
            SavingRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}