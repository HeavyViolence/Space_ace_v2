using Newtonsoft.Json;

using SpaceAce.Main.Saving;

using System;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelUnlocker : IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly ISavingSystem _savingSystem;
        private readonly LevelCompleter _levelCompleter;

        public int LargestCompletedLevelIndex { get; private set; }
        public int LargestUnlockedLevelIndex => LargestCompletedLevelIndex + 1;

        public string ID => "Game progress";

        public LevelUnlocker(ISavingSystem savingSystem, LevelCompleter levelCompleter)
        {
            _savingSystem = savingSystem ?? throw new ArgumentNullException();
            _levelCompleter = levelCompleter ?? throw new ArgumentNullException();
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

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

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