using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Main;
using SpaceAce.Main.Factories.PlayerShipFactories;
using SpaceAce.Main.Factories.SavedItemsFactories;
using SpaceAce.Main.Saving;
using SpaceAce.UI;

using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

using Zenject;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.Gameplay.Players
{
    public sealed class Player : IInitializable, IDisposable, ISavable, IFixedTickable, IEntityViewProvider
    {
        public event EventHandler ShipSpawned, ShipDefeated, SavingRequested;

        private static readonly JsonSerializerSettings s_serializationSettings = new()
        {
            SerializationBinder = new DefaultSerializationBinder(),
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
        };

        private readonly PlayerShipFactory _playerShipFactory;
        private readonly SavedItemsFactory _savedItemsFactory;
        private readonly Vector3 _shipSpawnPosition = Vector3.zero;
        private readonly ISavingSystem _savingSystem;
        private readonly GameStateLoader _gameStateLoader;
        private readonly GameControlsTransmitter _gameControlsTransmitter;
        private readonly GamePauser _gamePauser;

        private ShipCache _activeShip;
        private CancellationTokenSource _shootingCancellation;

        public Wallet Wallet { get; } = new();
        public Experience Experience { get; } = new();
        public Inventory Inventory { get; } = new();
        public PlayerShipType SelectedShipType { get; set; }

        public string SavedDataName => "Player";
        public IEntityView View => _activeShip.View;

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
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);

            Wallet.BalanceChanged += (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged += (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Inventory.ContentChanged += (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _gameStateLoader.LevelLoaded += async (_, _) => await SpawnPlayerShipAsync();
            _gameStateLoader.MainMenuLoaded += (_, _) => DisposePlayerShip();
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);

            Wallet.BalanceChanged -= (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged -= (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Inventory.ContentChanged -= (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _gameStateLoader.LevelLoaded -= async (_, _) => await SpawnPlayerShipAsync();
            _gameStateLoader.MainMenuLoaded -= (_, _) => DisposePlayerShip();
        }

        public string GetState()
        {
            PlayerState state = new(Wallet.Balance, Experience.Value, SelectedShipType, Inventory.GetItemsSavableStates());
            return JsonConvert.SerializeObject(state, s_serializationSettings);
        }

        public void SetState(string state)
        {
            try
            {
                PlayerState playerState = JsonConvert.DeserializeObject<PlayerState>(state, s_serializationSettings);

                Wallet.AddCredits(playerState.Credits);
                Experience.Add(playerState.Experience);
                SelectedShipType = playerState.SelectedShip;

                IEnumerable<IItem> inventoryContent = _savedItemsFactory.BatchCreate(playerState.InventoryContent);
                Inventory.TryAddItems(inventoryContent, out _);
            }
            catch (Exception)
            {
                PlayerState defaultState = PlayerState.Default;

                Wallet.ClearAndAddCredits(defaultState.Credits);
                SelectedShipType = defaultState.SelectedShip;
            }
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.SavedDataName == SavedDataName;

        public override int GetHashCode() => SavedDataName.GetHashCode();

        public void FixedTick()
        {
            if (_gameStateLoader.CurrentState != GameState.Level || _gamePauser.Paused == true) return;

            _activeShip.MovementController?.Move(_gameControlsTransmitter.MovementDirection);
            _activeShip.MovementController?.Rotate(_gameControlsTransmitter.MouseWorldPosition);
        }

        #endregion

        #region event handlers

        private async UniTask SpawnPlayerShipAsync()
        {
            _activeShip = _playerShipFactory.Create(SelectedShipType, _shipSpawnPosition, Quaternion.identity);
            ShipSpawned?.Invoke(this, EventArgs.Empty);

            await _activeShip.Shooting.BindInventoryAsync(Inventory);
            _activeShip.View.Destroyable.Destroyed += PlayerShipDestroyedEventHandler;

            _gameControlsTransmitter.SwitchToSmallGuns += async (_, _) => await _activeShip.Shooting.TrySwitchToGunsAsync(Size.Small);
            _gameControlsTransmitter.SwitchToMediumGuns += async (_, _) => await _activeShip.Shooting.TrySwitchToGunsAsync(Size.Medium);
            _gameControlsTransmitter.SwitchToLargeGuns += async (_, _) => await _activeShip.Shooting.TrySwitchToGunsAsync(Size.Large);

            _gameControlsTransmitter.SwitchToNextAmmo += async (_, _) => await _activeShip.Shooting.TrySwitchToNextAmmoAsync();
            _gameControlsTransmitter.SwitchToPreviousAmmo += async (_, _) => await _activeShip.Shooting.TrySwitchToPreviousAmmoAsync();

            _gameControlsTransmitter.Fire += FireEventHandler;
            _gameControlsTransmitter.Ceasefire += CeasefireEventHandler;
        }

        private void DisposePlayerShip()
        {
            _gameControlsTransmitter.SwitchToSmallGuns -= async (_, _) => await _activeShip.Shooting.TrySwitchToGunsAsync(Size.Small);
            _gameControlsTransmitter.SwitchToMediumGuns -= async (_, _) => await _activeShip.Shooting.TrySwitchToGunsAsync(Size.Medium);
            _gameControlsTransmitter.SwitchToLargeGuns -= async (_, _) => await _activeShip.Shooting.TrySwitchToGunsAsync(Size.Large);

            _gameControlsTransmitter.SwitchToNextAmmo -= async (_, _) => await _activeShip.Shooting.TrySwitchToNextAmmoAsync();
            _gameControlsTransmitter.SwitchToPreviousAmmo -= async (_, _) => await _activeShip.Shooting.TrySwitchToPreviousAmmoAsync();

            _gameControlsTransmitter.Fire -= FireEventHandler;
            _gameControlsTransmitter.Ceasefire -= CeasefireEventHandler;

            _activeShip.View.Destroyable.Destroyed -= PlayerShipDestroyedEventHandler;

            _activeShip.Dispose();
        }

        private void PlayerShipDestroyedEventHandler(object sender, DestroyedEventArgs e)
        {
            _gameControlsTransmitter.SwitchToSmallGuns -= async (_, _) => await _activeShip.Shooting.TrySwitchToGunsAsync(Size.Small);
            _gameControlsTransmitter.SwitchToMediumGuns -= async (_, _) => await _activeShip.Shooting.TrySwitchToGunsAsync(Size.Medium);
            _gameControlsTransmitter.SwitchToLargeGuns -= async (_, _) => await _activeShip.Shooting.TrySwitchToGunsAsync(Size.Large);

            _gameControlsTransmitter.SwitchToNextAmmo -= async (_, _) => await _activeShip.Shooting.TrySwitchToNextAmmoAsync();
            _gameControlsTransmitter.SwitchToPreviousAmmo -= async (_, _) => await _activeShip.Shooting.TrySwitchToPreviousAmmoAsync();

            _gameControlsTransmitter.Fire -= FireEventHandler;
            _gameControlsTransmitter.Ceasefire -= CeasefireEventHandler;

            _shootingCancellation?.Cancel();
            _shootingCancellation?.Dispose();
            _shootingCancellation = null;

            _activeShip.Dispose();

            ShipDefeated?.Invoke(this, e);
        }

        private void FireEventHandler(object sender, CallbackContext e)
        {
            _shootingCancellation = new();
            _activeShip.Shooting.FireAsync(this, _shootingCancellation.Token).Forget();
        }

        private void CeasefireEventHandler(object sender, CallbackContext e)
        {
            _shootingCancellation?.Cancel();
            _shootingCancellation?.Dispose();
        }

        #endregion
    }
}