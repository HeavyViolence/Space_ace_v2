using SpaceAce.Main;

using System;

using UnityEngine;
using UnityEngine.InputSystem;

using Zenject;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.Gameplay.Controls
{
    public sealed class GameControlsTransmitter : IInitializable, IDisposable
    {
        public event EventHandler<CallbackContext> GoToPreviousMenu;
        public event EventHandler<CallbackContext> OpenInventory;

        public event EventHandler<CallbackContext> SelectNextAmmo;
        public event EventHandler<CallbackContext> SelectPreviousAmmo;

        public event EventHandler<CallbackContext> SwitchToSmallWeapons;
        public event EventHandler<CallbackContext> SwitchToMediumWeapons;
        public event EventHandler<CallbackContext> SwitchToLargeWeapons;

        public event EventHandler<CallbackContext> Fire;
        public event EventHandler<CallbackContext> Ceasefire;

        private readonly GameControls _gameControls = new();
        private readonly GamePauser _gamePauser;
        private readonly GameStateLoader _gameStateLoader;
        private readonly MasterCameraHolder _masterCameraHolder;

        public Vector2 MovementDirection => _gameControls.Player.Movement.ReadValue<Vector2>();

        public Vector3 MouseWorldPosition
        {
            get
            {
                if (Mouse.current is null) return Vector3.zero;
                return _masterCameraHolder.MasterCamera.ScreenToWorldPoint(Mouse.current.position.value);
            }
        }

        public GameControlsTransmitter(GamePauser gamePauser,
                                       GameStateLoader gameStateLoader,
                                       MasterCameraHolder masterCameraHolder)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _masterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
        }

        #region interfaces

        public void Initialize()
        {
            _gamePauser.GamePaused += GamePausedEventHandler;
            _gamePauser.GameResumed += GameResumedEventHandler;

            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;

            _gameControls.Menu.Back.performed += (ctx) => GoToPreviousMenu?.Invoke(this, ctx);
            _gameControls.Menu.Inventory.performed += (ctx) => OpenInventory?.Invoke(this, ctx);

            _gameControls.Player.PreviousAmmo.performed += (ctx) => SelectPreviousAmmo?.Invoke(this, ctx);
            _gameControls.Player.NextAmmo.performed += (ctx) => SelectNextAmmo?.Invoke(this, ctx);

            _gameControls.Player.SwitchToSmallWeapons.performed += (ctx) => SwitchToSmallWeapons?.Invoke(this, ctx);
            _gameControls.Player.SwitchToMediumWeapons.performed += (ctx) => SwitchToMediumWeapons?.Invoke(this, ctx);
            _gameControls.Player.SwitchToLargeWeapons.performed += (ctx) => SwitchToLargeWeapons?.Invoke(this, ctx);

            _gameControls.Player.Fire.performed += (ctx) => Fire?.Invoke(this, ctx);
            _gameControls.Player.Fire.canceled += (ctx) => Ceasefire?.Invoke(this, ctx);

            _gameControls.Menu.Enable();
            _gameControls.Player.Disable();
        }

        public void Dispose()
        {
            _gamePauser.GamePaused -= GamePausedEventHandler;
            _gamePauser.GameResumed -= GameResumedEventHandler;

            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;

            _gameControls.Menu.Back.performed -= (ctx) => GoToPreviousMenu?.Invoke(this, ctx);
            _gameControls.Menu.Inventory.performed -= (ctx) => OpenInventory?.Invoke(this, ctx);

            _gameControls.Player.PreviousAmmo.performed -= (ctx) => SelectPreviousAmmo?.Invoke(this, ctx);
            _gameControls.Player.NextAmmo.performed -= (ctx) => SelectNextAmmo?.Invoke(this, ctx);

            _gameControls.Player.SwitchToSmallWeapons.performed -= (ctx) => SwitchToSmallWeapons?.Invoke(this, ctx);
            _gameControls.Player.SwitchToMediumWeapons.performed -= (ctx) => SwitchToMediumWeapons?.Invoke(this, ctx);
            _gameControls.Player.SwitchToLargeWeapons.performed -= (ctx) => SwitchToLargeWeapons?.Invoke(this, ctx);

            _gameControls.Player.Fire.performed -= (ctx) => Fire?.Invoke(this, ctx);
            _gameControls.Player.Fire.canceled -= (ctx) => Ceasefire?.Invoke(this, ctx);

            _gameControls.Disable();
            _gameControls.Dispose();
        }

        #endregion

        #region event handlers

        private void GamePausedEventHandler(object sender, EventArgs e)
        {
            _gameControls.Player.Disable();
        }

        private void GameResumedEventHandler(object sender, EventArgs e)
        {
            _gameControls.Player.Enable();
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            _gameControls.Player.Disable();
        }

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            _gameControls.Player.Enable();
        }

        #endregion
    }
}