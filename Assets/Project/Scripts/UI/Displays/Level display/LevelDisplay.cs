using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.Main.Localization;

using System;
using System.Threading;

using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI.Displays
{
    public sealed class LevelDisplay : UIDisplay
    {
        private readonly LevelDisplayConfig _config;

        protected override string DisplayHolderName => "Level display";

        private Font _localizedFont;

        private Label _levelCreditsRewardLabel;
        private Label _levelExperienceRewardLabel;
        private Label _levelStopwatchLabel;

        private Label _enemyCounterLabel;
        private Label _meteorCounterLabel;
        private Label _wreckCounterLabel;

        public LevelDisplay(VisualTreeAsset displayAsset,
                            PanelSettings settings,
                            Localizer localizer,
                            LevelDisplayConfig config) : base(displayAsset,
                                                              settings,
                                                              localizer)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;
        }

        public override async UniTask EnableAsync()
        {
            if (Active == true) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;
            DisplayedDocument.rootVisualElement.style.opacity = 0;

            _localizedFont = await Localizer.GetLocalizedFontAsync();

            CachePlayerDisplay();
            CacheTargetDisplay();

            _levelCreditsRewardLabel = DisplayedDocument.rootVisualElement.Q<Label>("Level-credits-reward-label");
            _levelCreditsRewardLabel.style.unityFont = _localizedFont;

            _levelExperienceRewardLabel = DisplayedDocument.rootVisualElement.Q<Label>("Level-experience-reward-label");
            _levelCreditsRewardLabel.style.unityFont = _localizedFont;

            _levelStopwatchLabel = DisplayedDocument.rootVisualElement.Q<Label>("Level-stopwatch-label");
            _levelStopwatchLabel.style.unityFont = _localizedFont;

            _enemyCounterLabel = DisplayedDocument.rootVisualElement.Q<Label>("Enemy-counter-label");
            _enemyCounterLabel.style.unityFont = _localizedFont;

            _meteorCounterLabel = DisplayedDocument.rootVisualElement.Q<Label>("Meteor-counter-label");
            _meteorCounterLabel.style.unityFont= _localizedFont;

            _wreckCounterLabel = DisplayedDocument.rootVisualElement.Q<Label>("Wreck-counter-label");
            _wreckCounterLabel.style.unityFont = _localizedFont;

            DisplayedDocument.rootVisualElement.style.opacity = 1;

            await base.EnableAsync();
        }

        public override async UniTask DisableAsync()
        {
            if (Active == false) return;

            _localizedFont = null;

            ClearPlayerDisplayCache();
            ClearTargetDisplayCache();

            _levelCreditsRewardLabel = null;
            _levelExperienceRewardLabel = null;
            _levelStopwatchLabel = null;

            _enemyCounterLabel = null;
            _meteorCounterLabel = null;
            _wreckCounterLabel = null;

            DisplayedDocument.visualTreeAsset = null;

            await base.DisableAsync();
        }

        #region player

        private VisualElement _playerShipPanel;
        private VisualElement _playerShipIcon;
        private Label _playerShipNameLabel;
        private VisualElement _playerShipDurabilityBar;
        private VisualElement _playerShipHeatBar;
        private Label _playerShipDurabilityLabel;
        private Label _playerShipArmorLabel;
        private Label _playerShipAmmoLabel;

        private string _ammoSizeCode, _ammoTypeCode;

        private void CachePlayerDisplay()
        {
            _playerShipPanel = DisplayedDocument.rootVisualElement.Q<VisualElement>("Player-ship-view-panel");
            _playerShipPanel.visible = false;

            _playerShipIcon = DisplayedDocument.rootVisualElement.Q<VisualElement>("Player-ship-icon");

            _playerShipNameLabel = DisplayedDocument.rootVisualElement.Q<Label>("Player-ship-name-label");
            _playerShipNameLabel.style.unityFont = _localizedFont;

            _playerShipDurabilityBar = DisplayedDocument.rootVisualElement.Q<VisualElement>("Player-ship-healthbar-foreground");
            _playerShipHeatBar = DisplayedDocument.rootVisualElement.Q<VisualElement>("Player-ship-heatbar-foreground");

            _playerShipDurabilityLabel = DisplayedDocument.rootVisualElement.Q<Label>("Player-ship-durability-label");
            _playerShipDurabilityLabel.style.unityFont = _localizedFont;

            _playerShipArmorLabel = DisplayedDocument.rootVisualElement.Q<Label>("Player-ship-armor-label");
            _playerShipArmorLabel.style.unityFont = _localizedFont;

            _playerShipAmmoLabel = DisplayedDocument.rootVisualElement.Q<Label>("Player-ship-ammo-label");
            _playerShipAmmoLabel.style.unityFont = _localizedFont;
        }

        private void ClearPlayerDisplayCache()
        {
            _playerShipPanel = null;
            _playerShipIcon = null;
            _playerShipDurabilityBar = null;
            _playerShipHeatBar = null;
            _playerShipHeatBar = null;
            _playerShipDurabilityLabel = null;
            _playerShipArmorLabel = null;
            _playerShipAmmoLabel = null;
        }

        public async UniTask DisplayPlayerShipViewAsync(IEntityView view, CancellationToken token = default)
        {
            if (Active == false) return;
            if (view is null) throw new ArgumentNullException();

            await UpdatePlayerShipVisualAsync(view);
            UpdatePlayerShipDurabilityView(view.DurabilityView);
            UpdatePlayerShipArmorView(view.ArmorView);
            await UpdatePlayerShipAmmoViewAsync(view.ShooterView, true);
            UpdatePlayerShipHeatView(view.ShooterView);

            _playerShipPanel.visible = true;

            view.DurabilityView.ValueChanged += (_, _) => UpdatePlayerShipDurabilityView(view.DurabilityView);
            view.DurabilityView.MaxValueChanged += (_, _) => UpdatePlayerShipDurabilityView(view.DurabilityView);

            view.ArmorView.ValueChanged += (_, _) => UpdatePlayerShipArmorView(view.ArmorView);

            view.ShooterView.GunsSwitched += async (_, _) => await UpdatePlayerShipAmmoViewAsync(view.ShooterView, true);
            view.ShooterView.ActiveAmmoView.OutOfAmmo += async (_, _) => await PlayerIsOutOfAmmoEventHandlerAsync();
            view.ShooterView.ActiveAmmoView.AmmoSwitched += async (_, _) => await UpdatePlayerShipAmmoViewAsync(view.ShooterView, true);
            view.ShooterView.ActiveAmmoView.AmountChanged += async (_, _) => await UpdatePlayerShipAmmoViewAsync(view.ShooterView, false);
            view.ShooterView.HeatValueChanged += (_, _) => UpdatePlayerShipHeatView(view.ShooterView);
            view.ShooterView.HeatCapacityChanged += (_, _) => UpdatePlayerShipHeatView(view.ShooterView);

            await UniTask.WaitUntil(() => token.IsCancellationRequested == true);

            view.DurabilityView.ValueChanged -= (_, _) => UpdatePlayerShipDurabilityView(view.DurabilityView);
            view.DurabilityView.MaxValueChanged -= (_, _) => UpdatePlayerShipDurabilityView(view.DurabilityView);

            view.ArmorView.ValueChanged -= (_, _) => UpdatePlayerShipArmorView(view.ArmorView);

            view.ShooterView.GunsSwitched -= async (_, _) => await UpdatePlayerShipAmmoViewAsync(view.ShooterView, true);
            view.ShooterView.ActiveAmmoView.OutOfAmmo -= async (_, _) => await PlayerIsOutOfAmmoEventHandlerAsync();
            view.ShooterView.ActiveAmmoView.AmmoSwitched -= async (_, _) => await UpdatePlayerShipAmmoViewAsync(view.ShooterView, true);
            view.ShooterView.ActiveAmmoView.AmountChanged -= async (_, _) => await UpdatePlayerShipAmmoViewAsync(view.ShooterView, false);
            view.ShooterView.HeatValueChanged -= (_, _) => UpdatePlayerShipHeatView(view.ShooterView);
            view.ShooterView.HeatCapacityChanged -= (_, _) => UpdatePlayerShipHeatView(view.ShooterView);

            if (Active == true) _playerShipPanel.visible = false;
        }

        private async UniTask UpdatePlayerShipVisualAsync(IEntityView view)
        {
            _playerShipNameLabel.text = await view.GetLocalizedNameAsync();
            _playerShipIcon.style.backgroundImage = view.Icon.texture;
        }

        private void UpdatePlayerShipDurabilityView(IDurabilityView view)
        {
            if (view.Regen == 0f) _playerShipDurabilityLabel.text = $"{view.MaxValue:n0}";
            else _playerShipDurabilityLabel.text = $"{view.MaxValue:n0}/+{view.Regen:n0}";

            Color durabilityColor = _config.GetDurabilityColor(view.ValueNormalized);

            _playerShipDurabilityBar.style.width = Length.Percent(view.ValuePercentage);
            _playerShipDurabilityBar.style.backgroundColor = new(durabilityColor);
        }

        private void UpdatePlayerShipArmorView(IArmorView view)
        {
            _playerShipArmorLabel.text = $"{view.Value:n0}";
        }

        private async UniTask UpdatePlayerShipAmmoViewAsync(IShooterView view, bool fullUpdate)
        {
            if (fullUpdate == true)
            {
                _ammoSizeCode = await view.ActiveAmmoView.GetSizeCodeAsync();
                _ammoTypeCode = await view.ActiveAmmoView.GetTypeCodeAsync();
            }

            _playerShipAmmoLabel.text = $"{_ammoSizeCode}.{_ammoTypeCode}.{view.ActiveAmmoView.Damage:n0}/{view.ActiveAmmoView.Amount:n0}";
        }

        private void UpdatePlayerShipHeatView(IShooterView view)
        {
            Color heatColor = view.Overheat == true ? _config.GetOverheatColor() : _config.GetHeatColor(view.HeatNormalized);

            _playerShipHeatBar.style.width = Length.Percent(view.HeatPercentage);
            _playerShipHeatBar.style.backgroundColor = new(heatColor);
        }

        private async UniTask PlayerIsOutOfAmmoEventHandlerAsync()
        {
            string warning = await Localizer.GetLocalizedStringAsync("Ammo", "None");
            _playerShipAmmoLabel.text = $"{_ammoSizeCode}.{warning}";
        }

        #endregion

        #region target

        private VisualElement _targetPanel;
        private VisualElement _targetIcon;
        private Label _targetNameLabel;
        private VisualElement _targetDurabilityBar;
        private Label _targetDurabilityLabel;
        private Label _targetArmorLabel;
        private Label _targetAmmoLabel;

        private Guid _targetViewID = Guid.Empty;
        private float _targetDisplayTimer = 0f;

        private void CacheTargetDisplay()
        {
            _targetPanel = DisplayedDocument.rootVisualElement.Q<VisualElement>("Target-view-panel");
            _targetPanel.visible = false;

            _targetIcon = DisplayedDocument.rootVisualElement.Q<VisualElement>("Target-icon");

            _targetNameLabel = DisplayedDocument.rootVisualElement.Q<Label>("Target-name-label");
            _targetNameLabel.style.unityFont = _localizedFont;

            _targetDurabilityBar = DisplayedDocument.rootVisualElement.Q<VisualElement>("Target-healthbar-foreground");

            _targetDurabilityLabel = DisplayedDocument.rootVisualElement.Q<Label>("Target-durability-label");
            _targetDurabilityLabel.style.unityFont = _localizedFont;

            _targetArmorLabel = DisplayedDocument.rootVisualElement.Q<Label>("Target-armor-label");
            _targetArmorLabel.style.unityFont = _localizedFont;

            _targetAmmoLabel = DisplayedDocument.rootVisualElement.Q<Label>("Target-ammo-label");
            _targetAmmoLabel.style.unityFont = _localizedFont;
        }

        private void ClearTargetDisplayCache()
        {
            _targetPanel = null;
            _targetIcon = null;
            _targetNameLabel = null;
            _targetDurabilityLabel = null;
            _targetDurabilityBar = null;
            _targetArmorLabel = null;
            _targetAmmoLabel = null;
        }

        public async UniTask DisplayTargetViewAsync(IEntityView view, CancellationToken token = default)
        {
            if (Active == false || _targetViewID == view.ID) return;
            if (view is null) throw new ArgumentNullException();

            await UpdateTargetVisualAsync(view);
            UpdateTargetDurabilityView(view.DurabilityView);
            UpdateTargetArmorView(view.ArmorView);
            await UpdateTargetAmmoViewAsync(view.ShooterView);

            _targetPanel.visible = true;
            _targetViewID = view.ID;
            _targetDisplayTimer = 0f;

            view.DurabilityView.ValueChanged += (_, _) => UpdateTargetDurabilityView(view.DurabilityView);
            view.DurabilityView.MaxValueChanged += (_, _) => UpdateTargetDurabilityView(view.DurabilityView);

            view.ArmorView.ValueChanged += (_, _) => UpdateTargetArmorView(view.ArmorView);

            view.DamageReceiver.DamageReceived += (_, _) => _targetDisplayTimer = 0f;

            if (view.ShooterView is not null)
            {
                view.ShooterView.GunsSwitched += async (_, _) => await UpdateTargetAmmoViewAsync(view.ShooterView);
                view.ShooterView.ActiveAmmoView.AmmoSwitched += async (_, _) => await UpdateTargetAmmoViewAsync(view.ShooterView);
            }

            while (_targetDisplayTimer < _config.TargetDisplayDuration)
            {
                if (token.IsCancellationRequested == true) break;

                _targetDisplayTimer += Time.deltaTime;

                await UniTask.Yield();
            }

            view.DurabilityView.ValueChanged -= (_, _) => UpdateTargetDurabilityView(view.DurabilityView);
            view.DurabilityView.MaxValueChanged += (_, _) => UpdateTargetDurabilityView(view.DurabilityView);

            view.ArmorView.ValueChanged -= (_, _) => UpdateTargetArmorView(view.ArmorView);

            view.DamageReceiver.DamageReceived -= (_, _) => _targetDisplayTimer = 0f;

            if (view.ShooterView is not null)
            {
                view.ShooterView.GunsSwitched -= async (_, _) => await UpdateTargetAmmoViewAsync(view.ShooterView);
                view.ShooterView.ActiveAmmoView.AmmoSwitched -= async (_, _) => await UpdateTargetAmmoViewAsync(view.ShooterView);
            }

            if (Active == true) _targetPanel.visible = false;
            _targetViewID = Guid.Empty;
        }

        private async UniTask UpdateTargetVisualAsync(IEntityView view)
        {
            _targetNameLabel.text = await view.GetLocalizedNameAsync();
            _targetIcon.style.backgroundImage = view.Icon.texture;
        }

        private void UpdateTargetDurabilityView(IDurabilityView view)
        {
            if (view.Regen == 0f) _targetDurabilityLabel.text = $"{view.MaxValue:n0}";
            else _targetDurabilityLabel.text = $"{view.MaxValue:n0}/+{view.Regen:n0})";

            _targetDurabilityBar.style.width = Length.Percent(view.ValuePercentage);

            Color color = _config.GetDurabilityColor(view.ValueNormalized);
            _targetDurabilityBar.style.backgroundColor = new(color);
        }

        private void UpdateTargetArmorView(IArmorView view)
        {
            _targetArmorLabel.text = $"{view.Value:n0}";
        }

        private async UniTask UpdateTargetAmmoViewAsync(IShooterView view)
        {
            if (view is null)
            {
                _targetAmmoLabel.text = await Localizer.GetLocalizedStringAsync("Guns", "None");
            }
            else
            {
                string ammoSizeCode = await view.ActiveAmmoView.GetSizeCodeAsync();
                string ammoTypeCode = await view.ActiveAmmoView.GetTypeCodeAsync();

                _targetAmmoLabel.text = $"{ammoSizeCode}.{ammoTypeCode}.{view.ActiveAmmoView.Damage:n0}";
            }
        }

        #endregion

        #region level stopwatch

        public void UpdateLevelStopwatch(LevelStopwatch stopwatch)
        {
            if (Active == false) return;
            if (stopwatch is null) throw new ArgumentNullException();

            _levelStopwatchLabel.text = $"{stopwatch.Minutes:d2}:{stopwatch.Seconds:d2}.{stopwatch.Milliseconds / 10:d2}";
        }

        #endregion

        #region level reward

        public void UpdateCreditsReward(float value)
        {
            if (Active == false) return;
            if (value < 0f) throw new ArgumentOutOfRangeException();

            _levelCreditsRewardLabel.text = $"{value:n0}";
        }

        public void UpdateExperienceReward(float value)
        {
            if (Active == false) return;
            if (value < 0f) throw new ArgumentOutOfRangeException();

            _levelExperienceRewardLabel.text = $"{value:n0}";
        }

        #endregion

        #region entity counters

        public void UpdateEnemyCounter(int defeated, int total)
        {
            _enemyCounterLabel.text = $"{defeated:n0}/{total:n0}";
        }

        public void UpdateMeteorCounter(int destroyed, int encountered)
        {
            _meteorCounterLabel.text = $"{destroyed:n0}/{encountered:n0}";
        }

        public void UpdateWreckCounter(int destroyed, int encountered)
        {
            _wreckCounterLabel.text = $"{destroyed:n0}/{encountered:n0}";
        }

        #endregion
    }
}