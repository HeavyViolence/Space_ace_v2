using Cysharp.Threading.Tasks;

using SpaceAce.Main.Localization;

using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public sealed class GamePauseDisplay : UIDisplay, IGamePauseDisplay
    {
        public event EventHandler ResumeButtonClicked;
        public event EventHandler InventoryButtonClicked;
        public event EventHandler SettingsButtonClicked;
        public event EventHandler MainMenuButtonClicked;
        public event EventHandler<PointerOverEvent> PointerOver;

        private Button _resumeButton;
        private Button _inventoryButton;
        private Button _settingsButton;
        private Button _mainMenuButton;

        protected override string DisplayHolderName => "Game pause display";

        public GamePauseDisplay(VisualTreeAsset displayAsset,
                                PanelSettings settings,
                                ILocalizer localizer) : base(displayAsset,
                                                             settings,
                                                             localizer) { }

        public override async UniTask EnableAsync()
        {
            if (Enabled == true) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;
            DisplayedDocument.rootVisualElement.style.opacity = 0f;

            Font localizedFont = await Localizer.GetLocalizedFontAsync();

            string localizedHeadlineText = await Localizer.GetLocalizedStringAsync("Game pause menu", "Headline text");

            Label headline = DisplayedDocument.rootVisualElement.Q<Label>("Headline-label");
            headline.style.unityFont = localizedFont;
            headline.text = localizedHeadlineText;

            string localizedResumeButtonText = await Localizer.GetLocalizedStringAsync("Game pause menu", "Resume button text");

            _resumeButton = DisplayedDocument.rootVisualElement.Q<Button>("Resume-button");
            _resumeButton.style.unityFont = localizedFont;
            _resumeButton.text = localizedResumeButtonText;

            _resumeButton.clicked += ResumeButtonClickedEventHandler;
            _resumeButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));

            string localizedInventoryButtonText = await Localizer.GetLocalizedStringAsync("Game pause menu", "Inventory button text");

            _inventoryButton = DisplayedDocument.rootVisualElement.Q<Button>("Inventory-button");
            _inventoryButton.style.unityFont = localizedFont;
            _inventoryButton.text = localizedInventoryButtonText;

            _inventoryButton.clicked += InventoryButtonClickedEventHandler;
            _inventoryButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));

            string localizedSettingsButtonText = await Localizer.GetLocalizedStringAsync("Game pause menu", "Settings button text");

            _settingsButton = DisplayedDocument.rootVisualElement.Q<Button>("Settings-button");
            _settingsButton.style.unityFont = localizedFont;
            _settingsButton.text = localizedSettingsButtonText;

            _settingsButton.clicked += SettingsButtonClickedEventHandler;
            _settingsButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));

            string localizedMainMenuButtonText = await Localizer.GetLocalizedStringAsync("Game pause menu", "Main menu button text");

            _mainMenuButton = DisplayedDocument.rootVisualElement.Q<Button>("Main-menu-button");
            _mainMenuButton.style.unityFont = localizedFont;
            _mainMenuButton.text = localizedMainMenuButtonText;

            _mainMenuButton.clicked += MainMenuButtonClickedEventHandler;
            _mainMenuButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));

            DisplayedDocument.rootVisualElement.style.opacity = 1f;
            Enabled = true;
        }

        public override void Disable()
        {
            if (Enabled == false) return;

            _resumeButton.clicked -= ResumeButtonClickedEventHandler;
            _resumeButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _resumeButton = null;

            _inventoryButton.clicked -= InventoryButtonClickedEventHandler;
            _inventoryButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _inventoryButton = null;

            _settingsButton.clicked -= SettingsButtonClickedEventHandler;
            _settingsButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _settingsButton = null;

            _mainMenuButton.clicked -= MainMenuButtonClickedEventHandler;
            _mainMenuButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _mainMenuButton = null;

            DisplayedDocument.visualTreeAsset = null;
            Enabled = false;
        }

        #region event handlers

        private void ResumeButtonClickedEventHandler()
        {
            ResumeButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void InventoryButtonClickedEventHandler()
        {
            InventoryButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void SettingsButtonClickedEventHandler()
        {
            SettingsButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void MainMenuButtonClickedEventHandler()
        {
            MainMenuButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void PointerOverEventHandler(PointerOverEvent e)
        {
            PointerOver?.Invoke(this, e);
        }

        #endregion
    }
}