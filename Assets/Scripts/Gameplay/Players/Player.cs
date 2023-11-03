using Newtonsoft.Json;
using SpaceAce.Architecture;
using SpaceAce.Gameplay.Controls;
using SpaceAce.Main;
using SpaceAce.Main.ObjectPooling;
using SpaceAce.Main.Saving;

using System;
using UnityEngine;

namespace SpaceAce.Gameplay.Players
{
    public sealed class Player : IDisposable, ISavable, IFixedUpdatable
    {
        public event EventHandler SpaceshipSpawned;
        public event EventHandler SpaceshipDefeated;
        public event EventHandler SavingRequested;

        private static readonly JsonSerializerSettings s_serializationSettings = new() { TypeNameHandling = TypeNameHandling.Auto };

        private readonly ISavingSystem _savingSystem = null;
        private readonly GameStateLoader _gameStateLoader = null;
        private readonly MultiobjectPool _multiobjectPool = null;
        private readonly ObjectPoolEntry _defaultShip = null;
        private readonly Vector3 _playerShipSpawnPosition = Vector3.zero;
        private readonly GameControlsTransmitter _gameControlsTransmitter = null;

        private IMovementController _playerShipMovementController = null;

        public Wallet Wallet { get; } = new();
        public Experience Experience { get; } = new();

        public string ID => "Player";

        public Player(ISavingSystem savingSystem,
                      GameStateLoader gameStateLoader,
                      MultiobjectPool multiobjectPool,
                      GameControlsTransmitter gameControlsTransmitter,
                      ObjectPoolEntry defaultShip,
                      Vector3 playerShipSpawnPosition)
        {
            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _multiobjectPool = multiobjectPool ?? throw new ArgumentNullException(nameof(multiobjectPool),
                $"Attempted to pass an empty {typeof(MultiobjectPool)}!");

            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException(nameof(gameControlsTransmitter),
                $"Attempted to pass an empty {typeof(GameControlsTransmitter)}!");

            if (defaultShip == null)
                throw new ArgumentNullException(nameof(defaultShip),
                    $"Attempted to pass an empty default player ship: {typeof(ObjectPoolEntry)}!");

            _defaultShip = defaultShip;
            _playerShipSpawnPosition = playerShipSpawnPosition;
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);

            Wallet.BalanceChanged += (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged += (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);

            Wallet.BalanceChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
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

        public void FixedUpdate()
        {

        }

        #endregion

        #region event handlers

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            SpawnPlayerShip();
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {

        }

        private void SpawnPlayerShip()
        {
            _defaultShip.EnsureObjectPoolExistence();

            GameObject playerShip = _multiobjectPool.GetObject(_defaultShip.AnchorName);
            playerShip.transform.position = _playerShipSpawnPosition;

            if (playerShip.TryGetComponent(out IMovementController controller) == true)
                _playerShipMovementController = controller;
            else
                throw new MissingComponentException($"Player ship prefab is missing a mandatory component: {typeof(IMovementController)}!");
        }

        #endregion
    }
}