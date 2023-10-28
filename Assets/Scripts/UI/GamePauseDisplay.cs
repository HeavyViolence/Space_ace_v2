using Cysharp.Threading.Tasks;

using SpaceAce.Architecture;
using SpaceAce.Gameplay.Controls;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Localization;

using System;

using UnityEngine;
using UnityEngine.UIElements;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.UI
{
    public sealed class GamePauseDisplay : UIDisplay, IInitializable, IDisposable
    {
        protected override string DisplayHolderName => "Game pause display";

        private readonly CachedService<MainMenuDisplay> _mainMenuDisplay = new();
        private readonly CachedService<LevelDisplay> _levelDisplay = new();

        private readonly GameControlsTransmitter _gameControlsTransmitter = null;
        private readonly GameStateLoader _gameStateLoader = null;
        private readonly GamePauser _gamePauser = null;

        private Button _resumeButton = null;
        private Button _inventoryButton = null;
        private Button _settingsButton = null;
        private Button _mainMenuButton = null;

        public GamePauseDisplay(VisualTreeAsset displayAsset,
                                PanelSettings settings,
                                UIAudio audio,
                                Localizer localizer,
                                GameControlsTransmitter gameControlsTransmitter,
                                GameStateLoader gameStateLoader,
                                GamePauser gamePauser) : base(displayAsset, settings, audio, localizer)
        {
            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException(nameof(gameControlsTransmitter),
                $"Attempted to pass an empty {typeof(GameControlsTransmitter)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(GameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");
        }

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
            _resumeButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler());

            string localizedInventoryButtonText = await Localizer.GetLocalizedStringAsync("Game pause menu", "Inventory button text");

            _inventoryButton = DisplayedDocument.rootVisualElement.Q<Button>("Inventory-button");
            _inventoryButton.style.unityFont = localizedFont;
            _inventoryButton.text = localizedInventoryButtonText;

            _inventoryButton.clicked += InventoryButtonClickedEventHandler;
            _inventoryButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler());

            string localizedSettingsButtonText = await Localizer.GetLocalizedStringAsync("Game pause menu", "Settings button text");

            _settingsButton = DisplayedDocument.rootVisualElement.Q<Button>("Settings-button");
            _settingsButton.style.unityFont = localizedFont;
            _settingsButton.text = localizedSettingsButtonText;

            _settingsButton.clicked += SettingsButtonClickedEventHandler;
            _settingsButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler());

            string localizedMainMenuButtonText = await Localizer.GetLocalizedStringAsync("Game pause menu", "Main menu button text");

            _mainMenuButton = DisplayedDocument.rootVisualElement.Q<Button>("Main-menu-button");
            _mainMenuButton.style.unityFont = localizedFont;
            _mainMenuButton.text = localizedMainMenuButtonText;

            _mainMenuButton.clicked += async () => await MainMenuButtonClickedEventHandlerAsync();
            _mainMenuButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler());

            DisplayedDocument.rootVisualElement.style.opacity = 1f;
            Enabled = true;
        }

        public override void Disable()
        {
            if (Enabled == false) return;

            _resumeButton.clicked -= ResumeButtonClickedEventHandler;
            _resumeButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler());
            _resumeButton = null;

            _inventoryButton.clicked -= InventoryButtonClickedEventHandler;
            _inventoryButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler());
            _inventoryButton = null;

            _settingsButton.clicked -= SettingsButtonClickedEventHandler;
            _settingsButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler());
            _settingsButton = null;

            _mainMenuButton.clicked -= async () => await MainMenuButtonClickedEventHandlerAsync();
            _mainMenuButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler());
            _mainMenuButton = null;

            DisplayedDocument.visualTreeAsset = null;
            Enabled = false;
        }

        #region interfaces

        public void Initialize()
        {
            _gameControlsTransmitter.GoToPreviousMenu += GoToPreviousMenuEventHandler;
            _gameControlsTransmitter.OpenInventory += OpenInventoryEventHandler;
        }

        public void Dispose()
        {
            _gameControlsTransmitter.GoToPreviousMenu -= GoToPreviousMenuEventHandler;
            _gameControlsTransmitter.OpenInventory -= OpenInventoryEventHandler;
        }

        #endregion

        #region event handlers

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (Enabled == true)
            {
                Disable();

                UIAudio.BackwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);

                _levelDisplay.Access.EnableAsync().Forget();
                _gamePauser.Resume();
            }
        }

        private void OpenInventoryEventHandler(object sender, CallbackContext e)
        {
            /*if (Enabled == true)
            {
                Disable();

                UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
            }*/
        }

        private void ResumeButtonClickedEventHandler()
        {
            Disable();

            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);

            _levelDisplay.Access.EnableAsync().Forget();
            _gamePauser.Resume();
        }

        private void InventoryButtonClickedEventHandler()
        {
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void SettingsButtonClickedEventHandler()
        {
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private async UniTask MainMenuButtonClickedEventHandlerAsync()
        {
            Disable();

            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);

            await _gameStateLoader.LoadMainMenuAsync();

            _gamePauser.Resume();
        }

        private void PointerOverEventHandler()
        {
            UIAudio.HoverOverAudio.PlayRandomAudioClip(Vector3.zero);
        }

        #endregion
    }
}