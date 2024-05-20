using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Controls;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;

using UnityEngine;

using Zenject;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.UI
{
    public sealed class LevelDisplayMediator : UIDisplayMediator, ITickable
    {
        private readonly LevelDisplayMediatorConfig _config;

        private readonly LevelDisplay _levelDisplay;
        private readonly GamePauseDisplay _gamePauseDisplay;
        private readonly GamePauser _gamePauser;
        private readonly GameControlsTransmitter _gameControlsTransmitter;
        private readonly Player _player;
        private readonly GameStateLoader _gameStateLoader;
        private readonly LevelStopwatch _levelStopwatch;
        private readonly EasingService _easingService;

        private float _playerShipMaxDurability;
        private float _playerShipDurabilityRegen;

        private float _playerShipArmor;

        private string _playerShipWeaponSizeCode;
        private string _playerShipAmmoTypeCode;
        private float _playerAmmoDamagePerSecond;

        public LevelDisplayMediator(AudioPlayer audioPlayer,
                                    UIAudio uiAudio,
                                    LevelDisplay levelDisplay,
                                    GamePauseDisplay gamePauseDisplay,
                                    GamePauser gamePauser,
                                    GameControlsTransmitter gameControlsTransmitter,
                                    Player player,
                                    GameStateLoader gameStateLoader,
                                    LevelStopwatch levelStopwatch,
                                    EasingService easingService,
                                    LevelDisplayMediatorConfig config) : base(audioPlayer, uiAudio)
        {
            _levelDisplay = levelDisplay ?? throw new ArgumentNullException();
            _gamePauseDisplay = gamePauseDisplay ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException();
            _player = player ?? throw new ArgumentNullException();
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _levelStopwatch = levelStopwatch ?? throw new ArgumentNullException();
            _easingService = easingService ?? throw new ArgumentNullException();

            if (config == null) throw new ArgumentNullException();
            _config = config;
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

            _levelDisplay.UpdateLevelStopwatchView(_levelStopwatch.Minutes, _levelStopwatch.Seconds, _levelStopwatch.Milliseconds);
            _levelDisplay.UpdatePlayerShipMaxDurabilityView(_playerShipMaxDurability, _playerShipDurabilityRegen);
            _levelDisplay.UpdatePlayerShipArmorView(_playerShipArmor);
            
            UpdatePlayerShipDurabilityBar();
            UpdatePlayerShipHeatBar();
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
            _player.ShipDefeated += PlayerDefeatedEventHandler;
            _levelDisplay.Enabled += async (s, e) => await LevelDisplayEnabledEventHandlerAsync(s, e);
            _levelDisplay.Disabled += LevelDisplayDisabledEventHandler;
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            _player.ShipDefeated -= PlayerDefeatedEventHandler;
            _levelDisplay.Enabled -= async (s, e) => await LevelDisplayEnabledEventHandlerAsync(s, e);
        }

        private async UniTask LevelDisplayEnabledEventHandlerAsync(object sender, EventArgs e)
        {
            _player.DurabilityView.MaxValueChanged += async (s, e) => await PlayerShipMaxDurabilityChangedEventHandlerAsync(s, e);
            _player.DurabilityView.RegenChanged += async (s, e) => await PlayerShipDurabilityRegenChangedEventHandlerAsync(s, e);

            _player.ArmorView.ValueChanged += async (s, e) => await PlayerShipArmorChangedEventHandlerAsync(s, e);

            _player.ShooterView.WeaponChanged += async (s, e) => await PlayerShipWeaponChangedEventHandlerAsync(s, e);
            _player.ShooterView.AmmoChanged += async (s, e) => await PlayerShipAmmoChangedEventHandlerAsync(s, e);
            _player.ShooterView.ActiveAmmo.AmountChanged += PlayerShipAmmoAmountChangedEventHandler;
            _player.ShooterView.OutOfAmmo += PlayerShipIsOutOfAmmoEventHandler;
            _player.ShooterView.Overheated += async (s, e) => await PlayerShipOverheatedEventHandlerAsync(s, e);

            _playerShipMaxDurability = _player.DurabilityView.MaxValue;
            _playerShipDurabilityRegen = _player.DurabilityView.Regen;

            _playerShipArmor = _player.ArmorView.Value;

            _playerShipWeaponSizeCode = await _player.ShooterView.FirstActiveGun.GetSizeCodeAsync();
            _playerShipAmmoTypeCode = await _player.ShooterView.ActiveAmmo.GetTypeCodeAsync();
            _playerAmmoDamagePerSecond = _player.ShooterView.GetDamagePerSecond();

            _levelDisplay.UpdatePlayerShipIcon(_player.DurabilityView.Icon);
            _levelDisplay.UpdatePlayerShipMaxDurabilityView(_player.DurabilityView.MaxValue, _player.DurabilityView.Regen);
            _levelDisplay.UpdatePlayerShipArmorView(_player.ArmorView.Value);
            _levelDisplay.UpdatePlayerShipAmmoView(_playerShipWeaponSizeCode,
                                                   _playerShipAmmoTypeCode,
                                                   _player.ShooterView.GetDamagePerSecond(),
                                                   _player.ShooterView.ActiveAmmo.Amount);

            UpdatePlayerShipDurabilityBar();
        }

        private void LevelDisplayDisabledEventHandler(object sender, EventArgs e)
        {
            _player.DurabilityView.MaxValueChanged -= async (s, e) => await PlayerShipMaxDurabilityChangedEventHandlerAsync(s, e);
            _player.DurabilityView.RegenChanged -= async (s, e) => await PlayerShipDurabilityRegenChangedEventHandlerAsync(s, e);

            _player.ArmorView.ValueChanged -= async (s, e) => await PlayerShipArmorChangedEventHandlerAsync(s, e);

            _player.ShooterView.WeaponChanged -= async (s, e) => await PlayerShipWeaponChangedEventHandlerAsync(s, e);
            _player.ShooterView.AmmoChanged -= async (s, e) => await PlayerShipAmmoChangedEventHandlerAsync(s, e);
            _player.ShooterView.ActiveAmmo.AmountChanged -= PlayerShipAmmoAmountChangedEventHandler;
            _player.ShooterView.OutOfAmmo -= PlayerShipIsOutOfAmmoEventHandler;
            _player.ShooterView.Overheated -= async (s, e) => await PlayerShipOverheatedEventHandlerAsync(s, e);
        }

        #region player

        private void PlayerDefeatedEventHandler(object sender, EventArgs e)
        {

        }

        private async UniTask PlayerShipMaxDurabilityChangedEventHandlerAsync(object sender, FloatValueChangedEventArgs e)
        {
            float timer = 0f;

            while (timer < _config.BarUpdateDuration)
            {
                timer += Time.deltaTime;

                float easingFactor = _easingService.GetEntranceFastEasingFactor(timer / _config.UpdateDuration);
                float valueToAddThisFrame = e.Delta * easingFactor * Time.deltaTime;

                _playerShipMaxDurability += valueToAddThisFrame;

                await UniTask.Yield();
            }
        }

        private async UniTask PlayerShipDurabilityRegenChangedEventHandlerAsync(object sender, FloatValueChangedEventArgs e)
        {
            float timer = 0f;

            while (timer < _config.UpdateDuration)
            {
                timer += Time.deltaTime;

                float easingFactor = _easingService.GetEntranceFastEasingFactor(timer / _config.UpdateDuration);
                float valueToAddThisFrame = e.Delta * easingFactor * Time.deltaTime;

                _playerShipDurabilityRegen += valueToAddThisFrame;

                await UniTask.Yield();
            }
        }

        private async UniTask PlayerShipArmorChangedEventHandlerAsync(object sender, FloatValueChangedEventArgs e)
        {
            float timer = 0f;

            while (timer < _config.UpdateDuration)
            {
                timer += Time.deltaTime;

                float easingFactor = _easingService.GetEntranceFastEasingFactor(timer / _config.UpdateDuration);
                float valueToAddThisFrame = e.Delta * easingFactor * Time.deltaTime;

                _playerShipArmor += valueToAddThisFrame;

                await UniTask.Yield();
            }
        }

        private async UniTask PlayerShipWeaponChangedEventHandlerAsync(object sender, WeaponChangedEventArgs e)
        {
            _playerAmmoDamagePerSecond = _player.ShooterView.GetDamagePerSecond();
            _playerShipWeaponSizeCode = await e.FirstActiveGun.GetSizeCodeAsync();
        }

        private async UniTask PlayerShipAmmoChangedEventHandlerAsync(object sender, AmmoChangedEventArgs e)
        {
            _playerShipAmmoTypeCode = await e.ActiveAmmo.GetTypeCodeAsync();

            e.ActiveAmmo.AmountChanged += PlayerShipAmmoAmountChangedEventHandler;
            e.PreviousAmmo.AmountChanged -= PlayerShipAmmoAmountChangedEventHandler;
        }

        private void PlayerShipAmmoAmountChangedEventHandler(object sender, IntValueChangedEventArgs e)
        {
            _levelDisplay.UpdatePlayerShipAmmoView(_playerShipWeaponSizeCode, _playerShipAmmoTypeCode, _playerAmmoDamagePerSecond, e.NewValue);
        }

        private void PlayerShipIsOutOfAmmoEventHandler(object sender, OutOfAmmoEventArgs e)
        {
            _levelDisplay.UpdatePlayerShipAmmoView(_playerShipWeaponSizeCode, _playerShipAmmoTypeCode, e.Warning);
        }

        private async UniTask PlayerShipOverheatedEventHandlerAsync(object sender, EventArgs e)
        {
            float timer = 0f;

            while (timer < _player.ShooterView.OverheatDuration)
            {
                timer += Time.deltaTime;

                float progressNormalized = timer / _player.ShooterView.OverheatDuration;
                float easingFactor = _easingService.GetExitSlowEasingFactor(progressNormalized);
                float overheatPercentage = (1f - progressNormalized) * 100f * easingFactor;
                float clampedOverheatPercentage = Mathf.Clamp(overheatPercentage, 0f, 100f);
                Color overheatColor = _config.GetHeatColor(1f);

                _levelDisplay.UpdatePlayerShipHeatBar(clampedOverheatPercentage, overheatColor);

                await UniTask.Yield();
            }
        }

        private void UpdatePlayerShipDurabilityBar()
        {
            if (_player.DurabilityView is null) return;

            Color durabilityColor = _config.GetDurabilityColor(_player.DurabilityView.ValueNormalized);
            _levelDisplay.UpdatePlayerShipDurabilityBar(_player.DurabilityView.ValuePercentage, durabilityColor);
        }

        private void UpdatePlayerShipHeatBar()
        {
            if (_player.ShooterView is null || _player.ShooterView.HeatNormalized == 1f) return;

            Color heatColor = _config.GetHeatColor(_player.ShooterView.HeatNormalized);
            _levelDisplay.UpdatePlayerShipHeatBar(_player.ShooterView.HeatPercentage, heatColor);
        }

        #endregion

        #endregion
    }
}