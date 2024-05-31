using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.Gameplay.Shooting.Ammo;
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
    public sealed class Player : IInitializable,
                                 IDisposable,
                                 ISavable,
                                 IFixedTickable,
                                 IAmmoObservable,
                                 IEntityView
    {
        public event EventHandler ShipSpawned, ShipDefeated, SavingRequested;

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

        private ShipCache _activeShip;
        private CancellationTokenSource _shootingCancellation;

        public Wallet Wallet { get; }
        public Experience Experience { get; }
        public Inventory Inventory { get; }
        public PlayerShipType SelectedShipType { get; set; }

        public string SavedDataName => "Player";

        public Shooting.Shooting Shooter => _activeShip.Shooting;

        public Guid ID => _activeShip.View.ID;
        public Sprite Icon => _activeShip?.View.Icon;

        public IDurabilityView DurabilityView => _activeShip?.View.DurabilityView;
        public IArmorView ArmorView => _activeShip?.View.ArmorView;
        public IShooterView ShooterView => _activeShip?.View.ShooterView;
        public IEscapable Escapable => _activeShip?.View.Escapable;
        public IDamageReceiver DamageReceiver => _activeShip?.View.DamageReceiver;
        public IDestroyable Destroyable => _activeShip?.View.Destroyable;

        public async UniTask<string> GetLocalizedNameAsync() => await _activeShip.View.GetLocalizedNameAsync();

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

            Wallet.BalanceChanged += (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged += (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Inventory.ContentChanged += (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);

            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);

            Wallet.BalanceChanged -= (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Experience.ValueChanged -= (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);
            Inventory.ContentChanged -= (_, _) => SavingRequested?.Invoke(this, EventArgs.Empty);

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

                IEnumerable<IItem> inventoryContent = _savedItemsFactory.BatchCreate(playerState.InventoryContent);
                Inventory.TryAddItems(inventoryContent, out _);
            }
            catch (Exception) { }
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.SavedDataName == SavedDataName;

        public override int GetHashCode() => SavedDataName.GetHashCode();

        public void FixedTick()
        {
            if (_gameStateLoader.CurrentState != GameState.Level || _gamePauser.Paused == true) return;

            _activeShip.Movement?.Move(_gameControlsTransmitter.MovementDirection);
            _activeShip.Movement?.Rotate(_gameControlsTransmitter.MouseWorldPosition);
        }

        #endregion

        #region event handlers

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            _shootingCancellation = new();
            SpawnPlayerShip();
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            ReleasePlayerShip();
        }

        private void SpawnPlayerShip()
        {
            _activeShip = _playerShipFactory.Create(SelectedShipType, _shipSpawnPosition, Quaternion.identity);

            _activeShip.Shooting.BindInventory(Inventory);
            _activeShip.View.Destroyable.Destroyed += PlayerShipDefeatedEventHandler;

            _gameControlsTransmitter.SwitchToSmallWeapons += async (sender, ctx) => await _activeShip.Shooting.TrySwitchWeaponsAsync(Size.Small);
            _gameControlsTransmitter.SwitchToMediumWeapons += async (sender, ctx) => await _activeShip.Shooting.TrySwitchWeaponsAsync(Size.Medium);
            _gameControlsTransmitter.SwitchToLargeWeapons += async (sender, ctx) => await _activeShip.Shooting.TrySwitchWeaponsAsync(Size.Large);

            _gameControlsTransmitter.SelectNextAmmo += async (sender, args) => await _activeShip.Shooting.TrySwitchToNextAmmoAsync();
            _gameControlsTransmitter.SelectPreviousAmmo += async (sender, args) => await _activeShip.Shooting.TrySwitchToPreviousAmmoAsync();

            _gameControlsTransmitter.Fire += FireEventHandler;
            _gameControlsTransmitter.Ceasefire += CeasefireEventHandler;

            ShipSpawned?.Invoke(this, EventArgs.Empty);
        }

        private void ReleasePlayerShip()
        {
            _gameControlsTransmitter.SwitchToSmallWeapons -= async (sender, ctx) => await _activeShip.Shooting.TrySwitchWeaponsAsync(Size.Small);
            _gameControlsTransmitter.SwitchToMediumWeapons -= async (sender, ctx) => await _activeShip.Shooting.TrySwitchWeaponsAsync(Size.Medium);
            _gameControlsTransmitter.SwitchToLargeWeapons -= async (sender, ctx) => await _activeShip.Shooting.TrySwitchWeaponsAsync(Size.Large);

            _gameControlsTransmitter.SelectNextAmmo -= async (sender, args) => await _activeShip.Shooting.TrySwitchToNextAmmoAsync();
            _gameControlsTransmitter.SelectPreviousAmmo -= async (sender, args) => await _activeShip.Shooting.TrySwitchToPreviousAmmoAsync();

            _gameControlsTransmitter.Fire -= FireEventHandler;
            _gameControlsTransmitter.Ceasefire -= CeasefireEventHandler;

            _activeShip.View.Destroyable.Destroyed -= PlayerShipDefeatedEventHandler;

            _playerShipFactory.Release(SelectedShipType, _activeShip);
        }

        private void PlayerShipDefeatedEventHandler(object sender, DestroyedEventArgs e)
        {
            _gameControlsTransmitter.SwitchToSmallWeapons -= async (sender, ctx) => await _activeShip.Shooting.TrySwitchWeaponsAsync(Size.Small);
            _gameControlsTransmitter.SwitchToMediumWeapons -= async (sender, ctx) => await _activeShip.Shooting.TrySwitchWeaponsAsync(Size.Medium);
            _gameControlsTransmitter.SwitchToLargeWeapons -= async (sender, ctx) => await _activeShip.Shooting.TrySwitchWeaponsAsync(Size.Large);

            _gameControlsTransmitter.SelectNextAmmo -= async (sender, args) => await _activeShip.Shooting.TrySwitchToNextAmmoAsync();
            _gameControlsTransmitter.SelectPreviousAmmo -= async (sender, args) => await _activeShip.Shooting.TrySwitchToPreviousAmmoAsync();

            _gameControlsTransmitter.Fire -= FireEventHandler;
            _gameControlsTransmitter.Ceasefire -= CeasefireEventHandler;

            _shootingCancellation?.Cancel();
            _shootingCancellation?.Dispose();
            _shootingCancellation = null;

            _playerShipFactory.Release(SelectedShipType, _activeShip);

            ShipDefeated?.Invoke(this, e);
        }

        private void FireEventHandler(object sender, CallbackContext e)
        {
            _shootingCancellation = new();
            _activeShip.Shooting.FireAsync(this, _shootingCancellation.Token).Forget();
        }

        private void CeasefireEventHandler(object sender, CallbackContext e)
        {
            _shootingCancellation.Cancel();
            _shootingCancellation.Dispose();
            _shootingCancellation = null;
        }

        #endregion
    }
}