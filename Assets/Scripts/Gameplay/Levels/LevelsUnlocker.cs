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

        public int LargestCompletedLevelIndex { get; private set; }
        public int LargestUnlockedLevelIndex => LargestCompletedLevelIndex + 1;

        public string ID => "Game progress";

        public LevelsUnlocker(ISavingSystem savingSystem , LevelsCompleter levelsCompleter)
        {
            if (savingSystem is null)
                throw new ArgumentNullException(nameof(savingSystem),
                    $"Attempted to pass an empty {typeof(ISavingSystem)}!");

            if (levelsCompleter is null)
                throw new ArgumentNullException(nameof(levelsCompleter),
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

        public string GetState() => JsonConvert.SerializeObject(LargestCompletedLevelIndex);

        public void SetState(string state)
        {
            try
            {
                int index = JsonConvert.DeserializeObject<int>(state);
                LargestCompletedLevelIndex = index;
            }
            catch (Exception)
            {
                LargestCompletedLevelIndex = 0;
            }
        }

        public override bool Equals(object obj) => Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && ID == other.ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion

        #region event handlers

        private void LevelCompletedEventHandler(object sender, LevelEndedEventArgs e)
        {
            LargestCompletedLevelIndex = e.LevelIndex;
            SavingRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}