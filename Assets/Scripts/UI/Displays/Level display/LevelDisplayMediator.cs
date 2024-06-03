using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Bombs;
using SpaceAce.Gameplay.Controls;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Gameplay.Meteors;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Wrecks;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.UI.Displays
{
    public sealed class LevelDisplayMediator : UIDisplayMediator, ITickable
    {
        private readonly LevelDisplay _levelDisplay;
        private readonly GamePauseDisplay _gamePauseDisplay;
        private readonly GamePauser _gamePauser;
        private readonly GameControlsTransmitter _gameControlsTransmitter;
        private readonly Player _player;
        private readonly GameStateLoader _gameStateLoader;
        private readonly LevelStopwatch _levelStopwatch;
        private readonly LevelRewardCollector _levelRewardCollector;
        private readonly MeteorSpawner _meteorSpawner;
        private readonly WreckSpawner _wreckSpawner;
        private readonly BombSpawner _bombSpawner;

        public LevelDisplayMediator(AudioPlayer audioPlayer,
                                    UIAudio uiAudio,
                                    LevelDisplay levelDisplay,
                                    GamePauseDisplay gamePauseDisplay,
                                    GamePauser gamePauser,
                                    GameControlsTransmitter gameControlsTransmitter,
                                    Player player,
                                    GameStateLoader gameStateLoader,
                                    LevelStopwatch levelStopwatch,
                                    LevelRewardCollector levelRewardCollector,
                                    MeteorSpawner meteorSpawner,
                                    WreckSpawner wreckSpawner,
                                    BombSpawner bombSpawner) : base(audioPlayer, uiAudio)
        {
            _levelDisplay = levelDisplay ?? throw new ArgumentNullException();
            _gamePauseDisplay = gamePauseDisplay ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException();
            _player = player ?? throw new ArgumentNullException();
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _levelStopwatch = levelStopwatch ?? throw new ArgumentNullException();
            _levelRewardCollector = levelRewardCollector ?? throw new ArgumentNullException();
            _meteorSpawner = meteorSpawner ?? throw new ArgumentNullException();
            _wreckSpawner = wreckSpawner ?? throw new ArgumentNullException();
            _bombSpawner = bombSpawner ?? throw new ArgumentNullException();
        }

        #region interfaces

        public override void Initialize()
        {
            _gameControlsTransmitter.GoToPreviousMenu += GoToPreviousMenuEventHandler;

            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
        }

        public override void Dispose()
        {
            _gameControlsTransmitter.GoToPreviousMenu -= GoToPreviousMenuEventHandler;

            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
        }

        public void Tick()
        {
            if (_gameStateLoader.CurrentState != GameState.Level && _gamePauser.Paused == true) return;

            _levelDisplay.UpdateLevelStopwatch(_levelStopwatch);
        }

        #endregion

        #region event handlers

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (_levelDisplay.Active == false) return;

            _levelDisplay.DisableAsync().Forget();
            _gamePauseDisplay.EnableAsync().Forget();
            _gamePauser.Pause();
            AudioPlayer.PlayOnceAsync(UIAudio.BackwardClick.Random, Vector3.zero).Forget();
        }

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            _levelDisplay.Enabled += LevelDisplayEnabledEventHandler;
            _levelDisplay.Disabled += LevelDisplayDisabledEventHandler;

            _player.ShipDefeated += PlayerShipDefeatedEventHandler;

            _meteorSpawner.MeteorSpawned += MeteorSpawnedEventHandler;
            _wreckSpawner.WreckSpawned += WreckSpawnedEventHandler;
            _bombSpawner.BombSpawned += BombSpawnedEventHandler;
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            _levelDisplay.Enabled -= LevelDisplayEnabledEventHandler;
            _levelDisplay.Disabled -= LevelDisplayDisabledEventHandler;

            _player.ShipDefeated -= PlayerShipDefeatedEventHandler;

            _meteorSpawner.MeteorSpawned -= MeteorSpawnedEventHandler;
            _wreckSpawner.WreckSpawned -= WreckSpawnedEventHandler;
            _bombSpawner.BombSpawned -= BombSpawnedEventHandler;

            _meteorsDestroyed = 0;
            _meteorsEncountered = 0;

            _wrecksDestroyed = 0;
            _wrecksEncountered = 0;
        }

        private void LevelDisplayEnabledEventHandler(object sender, EventArgs e)
        {
            _levelRewardCollector.CreditsRewardChanged += CreditsRewardChangedEventHandler;
            _levelRewardCollector.ExperienceRewardChanged += ExperienceRewardChangedEventHandler;

            _levelDisplay.UpdateMeteorCounter(_meteorsDestroyed, _meteorsEncountered);
            _levelDisplay.UpdateWreckCounter(_wrecksDestroyed, _wrecksEncountered);

            _levelDisplay.UpdateCreditsReward(_levelRewardCollector.CreditsReward);
            _levelDisplay.UpdateExperienceReward(_levelRewardCollector.ExperienceReward);

            _playerShipViewCancellation = new();
            _levelDisplay.DisplayPlayerShipViewAsync(_player.ShipView, _playerShipViewCancellation.Token).Forget();
        }

        private void LevelDisplayDisabledEventHandler(object sender, EventArgs e)
        {
            _levelRewardCollector.CreditsRewardChanged -= CreditsRewardChangedEventHandler;
            _levelRewardCollector.ExperienceRewardChanged -= ExperienceRewardChangedEventHandler;

            DisablePlayerShipViewIfActive();
            DisableMeteorViewIfActive();
            DisableWreckViewIfActive();
            DisableBombViewIfActive();
        }

        #region player

        private CancellationTokenSource _playerShipViewCancellation;

        private void PlayerShipDefeatedEventHandler(object sender, EventArgs e)
        {
            DisablePlayerShipViewIfActive();
        }

        private void DisablePlayerShipViewIfActive()
        {
            _playerShipViewCancellation?.Cancel();
            _playerShipViewCancellation?.Dispose();
            _playerShipViewCancellation = null;
        }

        #endregion

        #region level reward

        private void CreditsRewardChangedEventHandler(object sender, FloatValueChangedEventArgs e)
        {
            _levelDisplay.UpdateCreditsReward(e.NewValue);
        }

        private void ExperienceRewardChangedEventHandler(object sender, FloatValueChangedEventArgs e)
        {
            _levelDisplay.UpdateExperienceReward(e.NewValue);
        }

        #endregion

        #region meteors

        private CancellationTokenSource _meteorViewCancellation;
        private int _meteorsDestroyed = 0, _meteorsEncountered = 0;

        private void MeteorSpawnedEventHandler(object sender, MeteorSpawnedEventArgs e)
        {
            _levelDisplay.UpdateMeteorCounter(_meteorsDestroyed, ++_meteorsEncountered);

            e.View.DamageReceiver.DamageReceived += (_, _) =>
            {
                _meteorViewCancellation = new();
                _levelDisplay.DisplayTargetViewAsync(e.View, _meteorViewCancellation.Token).Forget();
            };

            e.View.Destroyable.Destroyed += (_, _) =>
            {
                _levelDisplay.UpdateMeteorCounter(++_meteorsDestroyed, _meteorsEncountered);
                DisableMeteorViewIfActive();
            };

            e.View.Escapable.Escaped += (_, _) => DisableMeteorViewIfActive();
        }

        private void DisableMeteorViewIfActive()
        {
            _meteorViewCancellation?.Cancel();
            _meteorViewCancellation?.Dispose();
            _meteorViewCancellation = null;
        }

        #endregion

        #region wrecks

        private CancellationTokenSource _wreckViewCancellation;
        private int _wrecksDestroyed = 0, _wrecksEncountered = 0;

        private void WreckSpawnedEventHandler(object sender, WreckSpawnedEventArgs e)
        {
            _levelDisplay.UpdateWreckCounter(_wrecksDestroyed, ++_wrecksEncountered);

            e.View.DamageReceiver.DamageReceived += (_, _) => 
            {
                _wreckViewCancellation = new();
                _levelDisplay.DisplayTargetViewAsync(e.View, _wreckViewCancellation.Token).Forget();
            };

            e.View.Destroyable.Destroyed += (_, _) =>
            {
                _levelDisplay.UpdateWreckCounter(++_wrecksDestroyed, _wrecksEncountered);
                DisableWreckViewIfActive();
            };

            e.View.Escapable.Escaped += (_, _) => DisableWreckViewIfActive();
        }

        private void DisableWreckViewIfActive()
        {
            _wreckViewCancellation?.Cancel();
            _wreckViewCancellation?.Dispose();
            _wreckViewCancellation = null;
        }

        #endregion

        #region bombs

        private CancellationTokenSource _bombViewCancellation;

        private void BombSpawnedEventHandler(object sender, BombSpawnedEventArgs e)
        {
            e.View.DamageReceiver.DamageReceived += (_, _) =>
            {
                _bombViewCancellation = new();
                _levelDisplay.DisplayTargetViewAsync(e.View, _bombViewCancellation.Token).Forget();
            };

            e.View.Destroyable.Destroyed += (_, _) => DisableBombViewIfActive();
            e.View.Escapable.Escaped += (_, _) => DisableBombViewIfActive();
        }

        private void DisableBombViewIfActive()
        {
            _bombViewCancellation?.Cancel();
            _bombViewCancellation?.Dispose();
            _bombViewCancellation = null;
        }

        #endregion

        #endregion
    }
}