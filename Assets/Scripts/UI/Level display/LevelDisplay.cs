using Cysharp.Threading.Tasks;

using SpaceAce.Main.Localization;

using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public sealed class LevelDisplay : UIDisplay
    {
        protected override string DisplayHolderName => "Level display";

        private Font _localizedFont;

        private VisualElement _playerShipIcon;
        private VisualElement _playerShipDurabilityBar;
        private VisualElement _playerShipHeatBar;
        private Label _playerShipDurabilityLabel;
        private Label _playerShipArmorLabel;
        private Label _playerShipAmmoLabel;

        private Label _levelCreditsRewardLabel;
        private Label _levelExperienceRewardLabel;
        private Label _levelStopwatchLabel;

        public LevelDisplay(VisualTreeAsset displayAsset,
                            PanelSettings settings,
                            Localizer localizer) : base(displayAsset,
                                                        settings,
                                                        localizer)
        { }

        public override async UniTask EnableAsync()
        {
            if (Active == true) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;
            DisplayedDocument.rootVisualElement.style.opacity = 0;

            _localizedFont = await Localizer.GetLocalizedFontAsync();

            _playerShipIcon = DisplayedDocument.rootVisualElement.Q<VisualElement>("Player-ship-icon");

            _playerShipDurabilityBar = DisplayedDocument.rootVisualElement.Q<VisualElement>("Player-ship-healthbar-foreground");
            _playerShipHeatBar = DisplayedDocument.rootVisualElement.Q<VisualElement>("Player-ship-heatbar-foreground");

            _playerShipDurabilityLabel = DisplayedDocument.rootVisualElement.Q<Label>("Player-ship-durability-label");
            _playerShipDurabilityLabel.style.unityFont = _localizedFont;

            _playerShipArmorLabel = DisplayedDocument.rootVisualElement.Q<Label>("Player-ship-armor-label");
            _playerShipArmorLabel.style.unityFont = _localizedFont;

            _playerShipAmmoLabel = DisplayedDocument.rootVisualElement.Q<Label>("Player-ship-ammo-label");
            _playerShipAmmoLabel.style.unityFont = _localizedFont;

            _levelCreditsRewardLabel = DisplayedDocument.rootVisualElement.Q<Label>("Level-credits-reward-label");
            _levelCreditsRewardLabel.style.unityFont = _localizedFont;

            _levelExperienceRewardLabel = DisplayedDocument.rootVisualElement.Q<Label>("Level-experience-reward-label");
            _levelCreditsRewardLabel.style.unityFont = _localizedFont;

            _levelStopwatchLabel = DisplayedDocument.rootVisualElement.Q<Label>("Level-stopwatch-label");
            _levelStopwatchLabel.style.unityFont = _localizedFont;

            DisplayedDocument.rootVisualElement.style.opacity = 1;

            await base.EnableAsync();
        }

        public override async UniTask DisableAsync()
        {
            if (Active == false) return;

            _localizedFont = null;

            _playerShipIcon = null;
            _playerShipDurabilityBar = null;
            _playerShipHeatBar = null;
            _playerShipDurabilityLabel = null;
            _playerShipArmorLabel = null;
            _playerShipAmmoLabel = null;

            _levelCreditsRewardLabel = null;
            _levelExperienceRewardLabel = null;

            _levelStopwatchLabel = null;

            DisplayedDocument.visualTreeAsset = null;

            await base.DisableAsync();
        }

        #region player

        public void UpdatePlayerShipIcon(Sprite icon)
        {
            if (Active == false) return;
            if (icon == null) throw new ArgumentNullException();

            _playerShipIcon.style.backgroundImage = icon.texture;
        }

        public void UpdatePlayerShipDurabilityView(float maxValue, float regen)
        {
            if (Active == false) return;

            if (maxValue <= 0f) throw new ArgumentOutOfRangeException();
            if (regen < 0f) throw new ArgumentOutOfRangeException();

            if (regen == 0f) _playerShipDurabilityLabel.text = $"{maxValue:n0}";
            else _playerShipDurabilityLabel.text = $"{maxValue:n0}(+{regen:n0})";
        }

        public void UpdatePlayerShipArmorView(float value)
        {
            if (Active == false) return;
            if (value < 0f) throw new ArgumentOutOfRangeException();

            _playerShipArmorLabel.text = $"{value:n0}";
        }
        
        public void UpdatePlayerShipAmmoView(string weaponSizeCode, string ammoTypeCode, float damagePerSecond, int ammoAmount)
        {
            if (Active == false) return;

            if (string.IsNullOrEmpty(weaponSizeCode) || string.IsNullOrWhiteSpace(weaponSizeCode))
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(ammoTypeCode) || string.IsNullOrWhiteSpace(ammoTypeCode))
                throw new ArgumentNullException();
            
            if (damagePerSecond <= 0f) throw new ArgumentOutOfRangeException();
            if (ammoAmount < 0) throw new ArgumentOutOfRangeException();

            _playerShipAmmoLabel.text = $"{weaponSizeCode}.{ammoTypeCode}.{damagePerSecond:n0}/{ammoAmount:n0}";
        }

        public void UpdatePlayerShipAmmoView(string weaponSizeCode, string ammoTypeCode, string warning)
        {
            if (Active == false) return;

            if (string.IsNullOrEmpty(weaponSizeCode) || string.IsNullOrWhiteSpace(weaponSizeCode))
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(ammoTypeCode) || string.IsNullOrWhiteSpace(ammoTypeCode))
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(warning) || string.IsNullOrWhiteSpace(warning))
                throw new ArgumentNullException();

            _playerShipAmmoLabel.text = $"{weaponSizeCode}.{ammoTypeCode}({warning})";
        }

        public void UpdatePlayerShipDurabilityBar(float percentage, Color color)
        {
            if (Active == false) return;
            if (percentage < 0f || percentage > 100f) throw new ArgumentOutOfRangeException();

            _playerShipDurabilityBar.style.width = Length.Percent(percentage);
            _playerShipDurabilityBar.style.backgroundColor = new(color);
        }

        public void UpdatePlayerShipHeatBar(float percentage, Color color)
        {
            if (Active == false) return;
            if (percentage < 0f || percentage > 100f) throw new ArgumentOutOfRangeException();

            _playerShipHeatBar.style.width = Length.Percent(percentage);
            _playerShipHeatBar.style.backgroundColor = new(color);
        }

        #endregion

        #region level stopwatch

        public void UpdateLevelStopwatch(int minutes, int seconds, int milliseconds)
        {
            if (Active == false) return;

            if (minutes < 0) throw new ArgumentOutOfRangeException();
            if (seconds < 0) throw new ArgumentOutOfRangeException();
            if (milliseconds < 0) throw new ArgumentOutOfRangeException();

            _levelStopwatchLabel.text = $"{minutes:d2}:{seconds:d2}.{milliseconds / 10:d2}";
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
    }
}