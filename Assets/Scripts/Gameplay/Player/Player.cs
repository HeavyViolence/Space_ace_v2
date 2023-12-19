using Newtonsoft.Json;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main;
using SpaceAce.Main.Factories;
using SpaceAce.Main.Saving;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Players
{
    public sealed class Player : IInitializable, IDisposable, ISavable, IFixedTickable
    {
        public event EventHandler SpaceshipSpawned;
        public event EventHandler SpaceshipDefeated;
        public event EventHandler SavingRequested;

        private static readonly JsonSerializerSettings s_serializationSettings = new() { TypeNameHandling = TypeNameHandling.Auto };

        private readonly PlayerShipFactory _playerShipFactory;
        private readonly ItemFactory _itemFactory;
        private readonly Vector3 _shipSpawnPosition = Vector3.zero;
        private readonly ISavingSystem _savingSystem;
        private readonly GameStateLoader _gameStateLoader;
        private readonly GameControlsTransmitter _gameControlsTransmitter;
        private readonly GamePauser _gamePauser;

        private GameObject _activeShip;
        private IMovementController _playerShipMovementController;
        private IShootingController _playerSHipShootingController;

        public Wallet Wallet { get; }
        public Experience Experience { get; }
        public Inventory Inventory { get; }
        public PlayerShipType SelectedShipType { get; set; }

        public string ID => "Player";

        public Player(PlayerShipFactory factory,
                      ItemFactory itemFactory,
                      Vector3 shipSpawnPosition,
                      float initialWalletBalance,
                      ISavingSystem savingSystem,
                      GameStateLoader gameStateLoader,
                      GameControlsTransmitter gameControlsTransmitter,
                      GamePauser gamePauser)
        {
            _playerShipFactory = factory ?? throw new ArgumentNullException(nameof(factory),
                $"Attempted to pass an empty {typeof(PlayerShipFactory)}!");

            _itemFactory = itemFactory ?? throw new ArgumentNullException(nameof(itemFactory),
                $"Attempted to pass an empty {typeof(ItemFactory)}!");

            _shipSpawnPosition = shipSpawnPosition;

            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException(nameof(gameControlsTransmitter),
                $"Attempted to pass an empty {typeof(GameControlsTransmitter)}!");

            Wallet = new(initialWalletBalance);
            Experience = new();
            Inventory = new();

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);

            Wallet.BalanceChanged += (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged += (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Inventory.ContentChanged += (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;

            _gameControlsTransmitter.Fire += (sender, args) => _playerSHipShootingController.Shoot();
            _gameControlsTransmitter.Ceasefire += (sender, args) => _playerSHipShootingController.StopShooting();
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);

            Wallet.BalanceChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Inventory.ContentChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;

            _gameControlsTransmitter.Fire -= (sender, args) => _playerSHipShootingController.Shoot();
            _gameControlsTransmitter.Ceasefire -= (sender, args) => _playerSHipShootingController.StopShooting();
        }

        public string GetState()
        {
            PlayerState state = new(Wallet.Balance, Experience.Value, SelectedShipType, Inventory.GetContentSnapshot());
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
                SelectedShipType = playerState.SelectedShip;

                IEnumerable<Item> items = _itemFactory.BatchCreate(playerState.InventorySnapshot);
                Inventory.AddRange(items);
            }
            catch (Exception) { }
        }

        public override bool Equals(object obj) => Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.ID == ID;

        public override int GetHashCode() => ID.GetHashCode();

        public void FixedTick()
        {
            if (_gameStateLoader.CurrentState != GameState.Level || _gamePauser.Paused == true) return;

            _playerShipMovementController.Move(_gameControlsTransmitter.MovementDirection);
            _playerShipMovementController.Rotate(_gameControlsTransmitter.MouseWorldPosition);
        }

        #endregion

        #region event handlers

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            SpawnPlayerShip();
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            ReleasePlayerShip();
        }

        private void SpawnPlayerShip()
        {
            _activeShip = _playerShipFactory.Create(SelectedShipType);
            _activeShip.transform.SetPositionAndRotation(_shipSpawnPosition, Quaternion.identity);

            if (_activeShip.TryGetComponent(out IMovementController movementController) == true)
                _playerShipMovementController = movementController;
            else throw new MissingComponentException($"Player ship is missing {typeof(IMovementController)}!");

            if (_activeShip.TryGetComponent(out IShootingController shootingController) == true)
                _playerSHipShootingController = shootingController;
            else throw new MissingComponentException($"Player ship is missing {typeof(IShootingController)}!");

            if (_activeShip.TryGetComponent(out IDestroyable destroyable) == true)
                destroyable.Destroyed += PlayerShipDefeatedEventHandler;
            else throw new MissingComponentException($"Player ship is missing {typeof(IDestroyable)}!");

            SpaceshipSpawned?.Invoke(this, EventArgs.Empty);
        }

        private void ReleasePlayerShip()
        {
            if (_activeShip == null) return;

            if (_activeShip.TryGetComponent(out IDestroyable destroyable) == true)
                destroyable.Destroyed -= PlayerShipDefeatedEventHandler;
            else throw new MissingComponentException($"Player ship is missing {typeof(IDestroyable)}!");

            _playerShipFactory.Release(_activeShip, SelectedShipType);

            _activeShip = null;
            _playerShipMovementController = null;
            _playerSHipShootingController = null;
        }

        private void PlayerShipDefeatedEventHandler(object sender, DestroyedEventArgs e)
        {
            _playerShipFactory.Release(_activeShip, SelectedShipType);
            SpaceshipDefeated?.Invoke(this, EventArgs.Empty);

            _activeShip = null;
            _playerShipMovementController = null;
            _playerSHipShootingController = null;
        }

        #endregion
    }
}