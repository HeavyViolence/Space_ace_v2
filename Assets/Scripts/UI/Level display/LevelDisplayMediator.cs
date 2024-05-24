using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Auxiliary.AnimatedValues;
using SpaceAce.Auxiliary.Easing;
using SpaceAce.Gameplay.Controls;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;
using System.Threading;

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
        private readonly LevelRewardCollector _levelRewardCollector;

        private readonly AnimatedFloat _levelCreditsReward;
        private readonly AnimatedFloat _levelExperienceReward;

        private readonly AnimatedFloat _playerShipMaxDurability;
        private readonly AnimatedFloat _playerShipDurabilityRegen;
        private readonly AnimatedFloat _playerShipArmor;

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
                                    LevelRewardCollector levelRewardCollector,
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
            _levelRewardCollector = levelRewardCollector ?? throw new ArgumentNullException();

            if (config == null) throw new ArgumentNullException();
            _config = config;

            _levelCreditsReward = new(easingService, _config.UpdateEasing, _config.UpdateDuration);
            _levelExperienceReward = new(easingService, _config.UpdateEasing, _config.UpdateDuration);

            _playerShipMaxDurability = new(easingService, _config.UpdateEasing, _config.UpdateDuration);
            _playerShipDurabilityRegen = new(easingService, _config.UpdateEasing, _config.UpdateDuration);
            _playerShipArmor = new(easingService, _config.UpdateEasing, _config.UpdateDuration);
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

            _levelCreditsReward.Update();
            _levelDisplay.UpdateCreditsReward(_levelCreditsReward.GetValue());

            _levelExperienceReward.Update();
            _levelDisplay.UpdateExperienceReward(_levelExperienceReward.GetValue());

            _levelDisplay.UpdateLevelStopwatch(_levelStopwatch.Minutes, _levelStopwatch.Seconds, _levelStopwatch.Milliseconds);

            _playerShipMaxDurability.Update();
            _playerShipDurabilityRegen.Update();
            _levelDisplay.UpdatePlayerShipDurabilityView(_playerShipMaxDurability.GetValue(), _playerShipDurabilityRegen.GetValue());

            _playerShipArmor.Update();
            _levelDisplay.UpdatePlayerShipArmorView(_playerShipArmor.GetValue());

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
            _levelDisplay.Enabled += async (s, e) => await LevelDisplayEnabledEventHandlerAsync(s, e);
            _levelDisplay.Disabled += LevelDisplayDisabledEventHandler;

            _player.ShipDefeated += PlayerDefeatedEventHandler;

            _levelCreditsReward.UnlockAdd(_levelRewardCollector.CreditsReward);
            _levelExperienceReward.UnlockAdd(_levelRewardCollector.ExperienceReward);

            _playerShipMaxDurability.UnlockAdd(_player.DurabilityView.MaxValue);
            _playerShipDurabilityRegen.UnlockAdd(_player.DurabilityView.Regen);
            _playerShipArmor.UnlockAdd(_player.ArmorView.Value);
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            _player.ShipDefeated -= PlayerDefeatedEventHandler;

            _levelDisplay.Enabled -= async (s, e) => await LevelDisplayEnabledEventHandlerAsync(s, e);
            _levelDisplay.Disabled -= LevelDisplayDisabledEventHandler;

            _levelCreditsReward.ResetLock();
            _levelExperienceReward.ResetLock();

            _playerShipMaxDurability.ResetLock();
            _playerShipDurabilityRegen.ResetLock();
            _playerShipArmor.ResetLock();
        }

        private async UniTask LevelDisplayEnabledEventHandlerAsync(object sender, EventArgs e)
        {
            _levelRewardCollector.CreditsRewardChanged += CreditsRewardChangedEventHandler;
            _levelRewardCollector.ExperienceRewardChanged += ExperienceRewardChangedEventHandler;

            _player.DurabilityView.MaxValueChanged += PlayerShipMaxDurabiltiyChangedEventHandler;
            _player.DurabilityView.RegenChanged += PlayerShipDurabilityRegenChangedEventHandler;

            _player.ArmorView.ValueChanged += PlayerShipArmorChangedEventHandler;

            _player.ShooterView.WeaponChanged += async (s, e) => await PlayerShipWeaponChangedEventHandlerAsync(s, e);
            _player.ShooterView.AmmoChanged += async (s, e) => await PlayerShipAmmoChangedEventHandlerAsync(s, e);
            _player.ShooterView.ActiveAmmo.AmountChanged += PlayerShipAmmoAmountChangedEventHandler;
            _player.ShooterView.OutOfAmmo += PlayerShipIsOutOfAmmoEventHandler;
            _player.ShooterView.Overheated += async (s, e) => await PlayerShipOverheatedEventHandlerAsync(s, e);

            _playerShipWeaponSizeCode = await _player.ShooterView.FirstActiveGun.GetSizeCodeAsync();
            _playerShipAmmoTypeCode = await _player.ShooterView.ActiveAmmo.GetTypeCodeAsync();
            _playerAmmoDamagePerSecond = _player.ShooterView.GetDamagePerSecond();

            _levelDisplay.UpdatePlayerShipIcon(_player.DurabilityView.Icon);
            _levelDisplay.UpdatePlayerShipDurabilityView(_player.DurabilityView.MaxValue, _player.DurabilityView.Regen);
            _levelDisplay.UpdatePlayerShipArmorView(_player.ArmorView.Value);
            _levelDisplay.UpdatePlayerShipAmmoView(_playerShipWeaponSizeCode,
                                                   _playerShipAmmoTypeCode,
                                                   _player.ShooterView.GetDamagePerSecond(),
                                                   _player.ShooterView.ActiveAmmo.Amount);
        }

        private void LevelDisplayDisabledEventHandler(object sender, EventArgs e)
        {
            _levelRewardCollector.CreditsRewardChanged -= CreditsRewardChangedEventHandler;
            _levelRewardCollector.ExperienceRewardChanged -= ExperienceRewardChangedEventHandler;

            _player.DurabilityView.MaxValueChanged -= PlayerShipMaxDurabiltiyChangedEventHandler;
            _player.DurabilityView.RegenChanged -= PlayerShipDurabilityRegenChangedEventHandler;

            _player.ArmorView.ValueChanged -= PlayerShipArmorChangedEventHandler;

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

        private void PlayerShipMaxDurabiltiyChangedEventHandler(object sender, FloatValueChangedEventArgs e)
        {
            _playerShipMaxDurability.Add(e.Delta);
        }

        private void PlayerShipDurabilityRegenChangedEventHandler(object sender, FloatValueChangedEventArgs e)
        {
            _playerShipDurabilityRegen.Add(e.Delta);
        }

        private void PlayerShipArmorChangedEventHandler(object sender, FloatValueChangedEventArgs e)
        {
            _playerShipArmor.Add(e.Delta);
        }

        #region player ship ammmo

        private async UniTask PlayerShipWeaponChangedEventHandlerAsync(object sender, WeaponChangedEventArgs e)
        {
            _playerAmmoDamagePerSecond = _player.ShooterView.GetDamagePerSecond();
            _playerShipWeaponSizeCode = await e.FirstActiveGun.GetSizeCodeAsync();
        }

        private async UniTask PlayerShipAmmoChangedEventHandlerAsync(object sender, AmmoChangedEventArgs e)
        {
            e.ActiveAmmo.AmountChanged += PlayerShipAmmoAmountChangedEventHandler;
            e.PreviousAmmo.AmountChanged -= PlayerShipAmmoAmountChangedEventHandler;

            _playerShipAmmoTypeCode = await e.ActiveAmmo.GetTypeCodeAsync();
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

                float progressNormalizedEased = _easingService.GetEasedTime(_config.BarUpdateEasing, timer / _player.ShooterView.OverheatDuration);
                float overheatPercentage = (1f - progressNormalizedEased) * 100f;
                float overheatPercentageClamped = Mathf.Clamp(overheatPercentage, 0f, 100f);
                Color overheatColor = _config.GetHeatColor(1f);

                _levelDisplay.UpdatePlayerShipHeatBar(overheatPercentageClamped, overheatColor);

                await UniTask.Yield();
            }
        }

        #endregion

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

        #region level reward

        private void CreditsRewardChangedEventHandler(object sender, FloatValueChangedEventArgs e)
        {
            _levelCreditsReward.Add(e.Delta);
        }

        private void ExperienceRewardChangedEventHandler(object sender, FloatValueChangedEventArgs e)
        {
            _levelExperienceReward.Add(e.Delta);
        }

        #endregion

        #endregion
    }
}