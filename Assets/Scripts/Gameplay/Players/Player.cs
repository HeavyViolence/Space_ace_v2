using Newtonsoft.Json;
using SpaceAce.Main;
using SpaceAce.Main.Saving;
using System;

namespace SpaceAce.Gameplay.Players
{
    public sealed class Player : IDisposable, ISavable
    {
        public event EventHandler SpaceshipSpawned;
        public event EventHandler SpaceshipDefeated;
        public event EventHandler SavingRequested;

        private static readonly JsonSerializerSettings s_serializationSettings = new() { TypeNameHandling = TypeNameHandling.Auto };

        private readonly ISavingSystem _savingSystem = null;
        private readonly LevelsLoader _levelsLoader = null;
        private readonly MainMenuLoader _mainMenuLoader = null;

        public Wallet Wallet { get; } = new();
        public Experience Experience { get; } = new();

        public string ID => "Player";

        public Player(ISavingSystem savingSystem,
                      LevelsLoader levelsLoader,
                      MainMenuLoader mainMenuLoader)
        {
            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");

            _levelsLoader = levelsLoader ?? throw new ArgumentNullException(nameof(levelsLoader),
                $"Attempted to pass an empty {typeof(LevelsLoader)}!");

            _mainMenuLoader = mainMenuLoader ?? throw new ArgumentNullException(nameof(mainMenuLoader),
                $"Attempted to pass an empty {typeof(MainMenuLoader)}!");
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);

            Wallet.BalanceChanged += (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged += (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _levelsLoader.LevelLoaded += LevelLoadedEventHandler;
            _mainMenuLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);

            Wallet.BalanceChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _levelsLoader.LevelLoaded -= LevelLoadedEventHandler;
            _mainMenuLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
        }

        public string GetState()
        {
            PlayerState state = new(Wallet.Balance, Experience.Value);
            string jsonState = JsonConvert.SerializeObject(state, s_serializationSettings);

            return jsonState;
        }

        public void SetState(string state)
        {
            try
            {
                PlayerState playerState = JsonConvert.DeserializeObject<PlayerState>(state, s_serializationSettings);

                Wallet.AddCredits(playerState.Credits);
                Experience.Add(playerState.Experience);
            }
            catch (Exception) { }
        }

        public override bool Equals(object obj) => Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.ID == ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion

        #region event handlers

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {

        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {

        }

        #endregion
    }
}