using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Main;
using SpaceAce.Main.Factories;
using SpaceAce.Main.Saving;

using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

using Zenject;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.Gameplay.Players
{
    public sealed class Player : IInitializable, IDisposable, ISavable, IFixedTickable
    {
        public event EventHandler ShipSpawned, ShipDefeated;
        public event EventHandler SavingRequested;

        private static readonly JsonSerializerSettings s_serializationSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto 
        };

        private readonly PlayerShipFactory _playerShipFactory;
        private readonly SavedItemsFactory _savedItemsFactory;
        private readonly Vector3 _shipSpawnPosition = Vector3.zero;
        private readonly ISavingSystem _savingSystem;
        private readonly GameStateLoader _gameStateLoader;
        private readonly GameControlsTransmitter _gameControlsTransmitter;
        private readonly GamePauser _gamePauser;

        private GameObject _activeShip;
        private IMovementController _playerShipMovementController;

        private Shooting.Shooting _playerShipShooting;
        private CancellationTokenSource _shootingCancellation;

        public Wallet Wallet { get; }
        public Experience Experience { get; }
        public Inventory Inventory { get; }
        public PlayerShipType SelectedShipType { get; set; }

        public string ID => "Player";

        public Player(PlayerShipFactory playerShipFactory,
                      SavedItemsFactory savedItemsFactory,
                      Vector3 shipSpawnPosition,
                      ISavingSystem savingSystem,
                      GameStateLoader gameStateLoader,
                      GameControlsTransmitter gameControlsTransmitter,
                      GamePauser gamePauser)
        {
            _playerShipFactory = playerShipFactory ?? throw new ArgumentNullException();
            _savedItemsFactory = savedItemsFactory ?? throw new ArgumentNullException();
            _shipSpawnPosition = shipSpawnPosition;
            _savingSystem = savingSystem ?? throw new ArgumentNullException();
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();

            Wallet = new();
            Experience = new();
            Inventory = new();
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
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);

            Wallet.BalanceChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Inventory.ContentChanged -= (sender, args) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
        }

        public string GetState()
        {
            PlayerState state = new(Wallet.Balance, Experience.Value, SelectedShipType, Inventory.GetItemsSavableStates());
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

                if (playerState.InventoryContent is not null)
                {
                    IEnumerable<IItem> content = _savedItemsFactory.BatchCreate(playerState.InventoryContent);
                    Inventory.TryAddItems(content, out _);
                }
            }
            catch (Exception) { }
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.ID == ID;

        public override int GetHashCode() => ID.GetHashCode();

        public void FixedTick()
        {
            if (_gameStateLoader.CurrentState != GameState.Level || _gamePauser.Paused == true) return;

            _playerShipMovementController?.Move(_gameControlsTransmitter.MovementDirection);
            _playerShipMovementController?.Rotate(_gameControlsTransmitter.MouseWorldPosition);
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
            _activeShip = _playerShipFactory.Create(SelectedShipType, _shipSpawnPosition, Quaternion.identity);

            if (_activeShip.TryGetComponent(out IMovementController movementController) == true)
                _playerShipMovementController = movementController;
            else throw new MissingComponentException($"Player ship is missing {typeof(IMovementController)}!");
            
            if (_activeShip.TryGetComponent(out Shooting.Shooting shooting) == true)
            {
                _playerShipShooting = shooting;
                _playerShipShooting.BindInventory(Inventory);
            }
            else
            {
                throw new MissingComponentException($"Player ship is missing {typeof(Shooting.Shooting)}!");
            }

            if (_activeShip.TryGetComponent(out IDestroyable destroyable) == true)
                destroyable.Destroyed += PlayerShipDefeatedEventHandler;
            else throw new MissingComponentException($"Player ship is missing {typeof(IDestroyable)}!");

            _gameControlsTransmitter.SwitchToSmallWeapons += async (sender, ctx) => await _playerShipShooting.TrySwitchWeaponsAsync(Size.Small);
            _gameControlsTransmitter.SwitchToMediumWeapons += async (sender, ctx) => await _playerShipShooting.TrySwitchWeaponsAsync(Size.Medium);
            _gameControlsTransmitter.SwitchToLargeWeapons += async (sender, ctx) => await _playerShipShooting.TrySwitchWeaponsAsync(Size.Large);

            _gameControlsTransmitter.SelectNextAmmo += async (sender, args) => await _playerShipShooting.TrySwitchToNextAmmoAsync();
            _gameControlsTransmitter.SelectPreviousAmmo += async (sender, args) => await _playerShipShooting.TrySwitchToPreviousAmmoAsync();

            _gameControlsTransmitter.Fire += FireEventHandler;
            _gameControlsTransmitter.Ceasefire += CeasefireEventHandler;

            ShipSpawned?.Invoke(this, EventArgs.Empty);
        }

        private void ReleasePlayerShip()
        {
            if (_activeShip == null) return;

            _gameControlsTransmitter.SwitchToSmallWeapons -= async (sender, ctx) => await _playerShipShooting.TrySwitchWeaponsAsync(Size.Small);
            _gameControlsTransmitter.SwitchToMediumWeapons -= async (sender, ctx) => await _playerShipShooting.TrySwitchWeaponsAsync(Size.Medium);
            _gameControlsTransmitter.SwitchToLargeWeapons -= async (sender, ctx) => await _playerShipShooting.TrySwitchWeaponsAsync(Size.Large);

            _gameControlsTransmitter.SelectNextAmmo -= async (sender, args) => await _playerShipShooting.TrySwitchToNextAmmoAsync();
            _gameControlsTransmitter.SelectPreviousAmmo -= async (sender, args) => await _playerShipShooting.TrySwitchToPreviousAmmoAsync();

            _gameControlsTransmitter.Fire -= FireEventHandler;
            _gameControlsTransmitter.Ceasefire -= CeasefireEventHandler;

            if (_activeShip.TryGetComponent(out IDestroyable destroyable) == true)
                destroyable.Destroyed -= PlayerShipDefeatedEventHandler;
            else throw new MissingComponentException($"Player ship is missing {typeof(IDestroyable)}!");

            _playerShipFactory.Release(_activeShip, SelectedShipType);

            _activeShip = null;
            _playerShipMovementController = null;
            _playerShipShooting = null;
        }

        private void PlayerShipDefeatedEventHandler(object sender, DestroyedEventArgs e)
        {
            _gameControlsTransmitter.SwitchToSmallWeapons -= async (sender, ctx) => await _playerShipShooting.TrySwitchWeaponsAsync(Size.Small);
            _gameControlsTransmitter.SwitchToMediumWeapons -= async (sender, ctx) => await _playerShipShooting.TrySwitchWeaponsAsync(Size.Medium);
            _gameControlsTransmitter.SwitchToLargeWeapons -= async (sender, ctx) => await _playerShipShooting.TrySwitchWeaponsAsync(Size.Large);

            _gameControlsTransmitter.SelectNextAmmo -= async (sender, args) => await _playerShipShooting.TrySwitchToNextAmmoAsync();
            _gameControlsTransmitter.SelectPreviousAmmo -= async (sender, args) => await _playerShipShooting.TrySwitchToPreviousAmmoAsync();

            _gameControlsTransmitter.Fire -= FireEventHandler;
            _gameControlsTransmitter.Ceasefire -= CeasefireEventHandler;

            _shootingCancellation.Cancel();
            _shootingCancellation.Dispose();

            _playerShipFactory.Release(_activeShip, SelectedShipType);
            ShipDefeated?.Invoke(this, EventArgs.Empty);

            _activeShip = null;
            _playerShipMovementController = null;
            _playerShipShooting = null;
        }

        private void FireEventHandler(object sender, CallbackContext e)
        {
            _shootingCancellation = new();
            _playerShipShooting?.FireAsync(this, _shootingCancellation.Token).Forget();
        }

        private void CeasefireEventHandler(object sender, CallbackContext e)
        {
            _shootingCancellation.Cancel();
            _shootingCancellation = null;
        }

        #endregion
    }
}