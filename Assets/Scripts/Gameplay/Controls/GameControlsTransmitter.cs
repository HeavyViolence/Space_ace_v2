using SpaceAce.Architecture;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Main;

using System;

using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.Gameplay.Controls
{
    public sealed class GameControlsTransmitter : IInitializable, IDisposable
    {
        public event EventHandler<CallbackContext> GoToPreviousMenu;
        public event EventHandler<CallbackContext> OpenInventory;

        public event EventHandler<CallbackContext> SelectNextAmmo;
        public event EventHandler<CallbackContext> SelectPreviousAmmo;

        public event EventHandler<CallbackContext> Fire;
        public event EventHandler<CallbackContext> Ceasefire;

        private readonly GameControls _gameControls = new();

        private readonly GamePauser _gamePauser = null;
        private readonly GameStateLoader _gameStateLoader = null;
        private readonly LevelsCompleter _levelsCompleter = null;
        private readonly MasterCameraHolder _masterCameraHolder = null;

        public Vector2 MovementDirection => _gameControls.Player.Movement.ReadValue<Vector2>();

        public Vector3 MouseWorldPosition
        {
            get
            {
                if (Mouse.current is null) return Vector3.zero;

                Vector2 mouseScreenPosition = Mouse.current.position.value;

                Vector3 mouseScreenPositionWithDepth = new(mouseScreenPosition.x,
                                                           mouseScreenPosition.y,
                                                           _masterCameraHolder.MasterCamera.transform.position.z);

                return _masterCameraHolder.MasterCamera.ScreenToWorldPoint(mouseScreenPositionWithDepth);
            }
        }

        public GameControlsTransmitter(GamePauser gamePauser,
                                       GameStateLoader gameStateLoader,
                                       LevelsCompleter levelsCompleter,
                                       MasterCameraHolder masterCameraHolder)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an mepty {typeof(GameStateLoader)}!");

            _levelsCompleter = levelsCompleter ?? throw new ArgumentNullException(nameof(levelsCompleter),
                $"Attempted to pass an empty {typeof(LevelsCompleter)}!");

            _masterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException(nameof(masterCameraHolder),
                $"Attempted to pass an empty {typeof(MasterCameraHolder)}!");
        }

        #region interfaces

        public void Initialize()
        {
            _gamePauser.GamePaused += GamePausedEventHandler;
            _gamePauser.GameResumed += GameResumedEventHandler;

            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;

            _levelsCompleter.LevelConcluded += LevelConcludedEventHandler;

            _gameControls.Menu.Back.performed += (ctx) => GoToPreviousMenu?.Invoke(this, ctx);
            _gameControls.Menu.Inventory.performed += (ctx) => OpenInventory?.Invoke(this, ctx);

            _gameControls.Player.PreviousAmmo.performed += (ctx) => SelectPreviousAmmo?.Invoke(this, ctx);
            _gameControls.Player.NextAmmo.performed += (ctx) => SelectNextAmmo?.Invoke(this, ctx);

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

            _levelsCompleter.LevelConcluded -= LevelConcludedEventHandler;

            _gameControls.Menu.Back.performed -= (ctx) => GoToPreviousMenu?.Invoke(this, ctx);
            _gameControls.Menu.Inventory.performed -= (ctx) => OpenInventory?.Invoke(this, ctx);

            _gameControls.Player.PreviousAmmo.performed -= (ctx) => SelectPreviousAmmo?.Invoke(this, ctx);
            _gameControls.Player.NextAmmo.performed -= (ctx) => SelectNextAmmo?.Invoke(this, ctx);

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

        private void LevelConcludedEventHandler(object sender, LevelEndedEventArgs e)
        {
            _gameControls.Player.Disable();
        }

        #endregion
    }
}