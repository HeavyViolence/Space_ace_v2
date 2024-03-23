using Cysharp.Threading.Tasks;

using SpaceAce.Main.Localization;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

using System;

namespace SpaceAce.UI
{
    public sealed class MainMenuDisplay : UIDisplay
    {
        public event EventHandler PlayButtonClicked;
        public event EventHandler InventoryButtonClicked;
        public event EventHandler ArmoryButtonClicked;
        public event EventHandler SettingsButtonClicked;
        public event EventHandler StatisticsButtonClicked;
        public event EventHandler CreditsButtonClicked;
        public event EventHandler CheatsButtonClicked;
        public event EventHandler<PointerOverEvent> PointerOver;

        private Button _playButton;
        private Button _inventoryButton;
        private Button _armoryButton;
        private Button _settingsButton;
        private Button _statisticsButton;
        private Button _creditsButton;
        private Button _cheatsButton;

        protected override string DisplayHolderName => "Main menu display";

        public MainMenuDisplay(VisualTreeAsset displayAsset,
                               PanelSettings settings,
                               Localizer localizer) : base(displayAsset, settings, localizer) { }

        public override async UniTask EnableAsync()
        {
            if (Enabled == true) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;
            DisplayedDocument.rootVisualElement.style.opacity = 0f;

            _playButton = DisplayedDocument.rootVisualElement.Q<Button>("Play-button");
            _playButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _playButton.clicked += PlayButtonClickedEventHandler;

            _inventoryButton = DisplayedDocument.rootVisualElement.Q<Button>("Inventory-button");
            _inventoryButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _inventoryButton.clicked += InventoryButtonClickedEventHandler;

            _armoryButton = DisplayedDocument.rootVisualElement.Q<Button>("Armory-button");
            _armoryButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _armoryButton.clicked += ArmoryButtonClickedEventHandler;

            _settingsButton = DisplayedDocument.rootVisualElement.Q<Button>("Settings-button");
            _settingsButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _settingsButton.clicked += SettingsButtonClickedEventHandler;

            _statisticsButton = DisplayedDocument.rootVisualElement.Q<Button>("Statistics-button");
            _statisticsButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _statisticsButton.clicked += StatisticsButtonClickedEventHandler;

            _creditsButton = DisplayedDocument.rootVisualElement.Q<Button>("Credits-button");
            _creditsButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _creditsButton.clicked += CreditsButtonClickedEventHandler;

            _cheatsButton = DisplayedDocument.rootVisualElement.Q<Button>("Cheats-button");
            _cheatsButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _cheatsButton.clicked += CheatsButtonClickedEventHandler;

            var operation = LocalizationSettings.InitializationOperation;
            await UniTask.WaitUntil(() => operation.IsDone == true);

            string localizedPlayButtonText = await Localizer.GetLocalizedStringAsync("Main menu", "Play button text");
            string localizedInventoryButtonText = await Localizer.GetLocalizedStringAsync("Main menu", "Inventory button text");
            string localizedArmoryButtonText = await Localizer.GetLocalizedStringAsync("Main menu", "Armory button text");
            string localizedSettingsButtonText = await Localizer.GetLocalizedStringAsync("Main menu", "Settings button text");
            string localizedStatisticsButtonText = await Localizer.GetLocalizedStringAsync("Main menu", "Statistics button text");
            string localizedCredistButtonText = await Localizer.GetLocalizedStringAsync("Main menu", "Credits button text");
            string localizedCheatsButtonText = await Localizer.GetLocalizedStringAsync("Main menu", "Cheats button text");

            Font localizedFont = await Localizer.GetLocalizedFontAsync();

            _playButton.text = localizedPlayButtonText;
            _playButton.style.unityFont = localizedFont;

            _inventoryButton.text = localizedInventoryButtonText;
            _inventoryButton.style.unityFont = localizedFont;

            _armoryButton.text = localizedArmoryButtonText;
            _armoryButton.style.unityFont = localizedFont;

            _settingsButton.text = localizedSettingsButtonText;
            _settingsButton.style.unityFont = localizedFont;

            _statisticsButton.text = localizedStatisticsButtonText;
            _statisticsButton.style.unityFont = localizedFont;

            _creditsButton.text = localizedCredistButtonText;
            _creditsButton.style.unityFont = localizedFont;

            _cheatsButton.text = localizedCheatsButtonText;
            _cheatsButton.style.unityFont = localizedFont;

            DisplayedDocument.rootVisualElement.style.opacity = 1f;
            Enabled = true;
        }

        public override void Disable()
        {
            if (Enabled == false) return;

            _playButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _playButton.clicked -= PlayButtonClickedEventHandler;
            _playButton = null;

            _inventoryButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _inventoryButton.clicked -= InventoryButtonClickedEventHandler;
            _inventoryButton = null;

            _armoryButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _armoryButton.clicked -= ArmoryButtonClickedEventHandler;
            _armoryButton = null;

            _settingsButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _settingsButton.clicked -= SettingsButtonClickedEventHandler;
            _settingsButton = null;

            _statisticsButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _statisticsButton.clicked -= StatisticsButtonClickedEventHandler;
            _statisticsButton = null;

            _creditsButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _creditsButton.clicked -= CreditsButtonClickedEventHandler;
            _creditsButton = null;

            _cheatsButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _cheatsButton.clicked -= CheatsButtonClickedEventHandler;
            _cheatsButton = null;

            DisplayedDocument.visualTreeAsset = null;
            Enabled = false;
        }

        #region event handlers

        private void PointerOverEventHandler(PointerOverEvent e)
        {
            PointerOver?.Invoke(this, e);
        }

        private void PlayButtonClickedEventHandler()
        {
            PlayButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void InventoryButtonClickedEventHandler()
        {
            InventoryButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ArmoryButtonClickedEventHandler()
        {
            ArmoryButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void SettingsButtonClickedEventHandler()
        {
            SettingsButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void StatisticsButtonClickedEventHandler()
        {
            StatisticsButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void CreditsButtonClickedEventHandler()
        {
            CreditsButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void CheatsButtonClickedEventHandler()
        {
            CheatsButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}