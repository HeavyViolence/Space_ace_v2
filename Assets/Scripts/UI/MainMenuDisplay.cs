using Cysharp.Threading.Tasks;

using SpaceAce.Architecture;
using SpaceAce.Main.Localization;
using SpaceAce.Main.Audio;
using SpaceAce.Main;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

using System;

namespace SpaceAce.UI
{
    public sealed class MainMenuDisplay : UIDisplay, IInitializable, IDisposable
    {
        private readonly CachedService<LevelSelectionDisplay> _levelSelectionDisplay = new();

        private readonly GameStateLoader _gameStateLoader = null;

        private Button _playButton = null;
        private Button _inventoryButton = null;
        private Button _armoryButton = null;
        private Button _settingsButton = null;
        private Button _statisticsButton = null;
        private Button _creditsButton = null;
        private Button _cheatsButton = null;

        protected override string DisplayHolderName => "Main menu display";

        public MainMenuDisplay(VisualTreeAsset displayAsset,
                               PanelSettings settings,
                               UIAudio audio,
                               Localizer localizer,
                               GameStateLoader gameStateLoader) : base(displayAsset, settings, audio, localizer)
        {
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(GameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");
        }

        public override async UniTask EnableAsync()
        {
            if (Enabled == true) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;
            DisplayedDocument.rootVisualElement.style.opacity = 0f;

            _playButton = DisplayedDocument.rootVisualElement.Q<Button>("Play-button");
            _playButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _playButton.clicked += PlayButtonClickedEventHandler;

            _inventoryButton = DisplayedDocument.rootVisualElement.Q<Button>("Inventory-button");
            _inventoryButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _inventoryButton.clicked += InventoryButtonClickedEventHandler;

            _armoryButton = DisplayedDocument.rootVisualElement.Q<Button>("Armory-button");
            _armoryButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _armoryButton.clicked += ArmoryButtonClickedEventHandler;

            _settingsButton = DisplayedDocument.rootVisualElement.Q<Button>("Settings-button");
            _settingsButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _settingsButton.clicked += SettingsButtonClickedEventHandler;

            _statisticsButton = DisplayedDocument.rootVisualElement.Q<Button>("Statistics-button");
            _statisticsButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _statisticsButton.clicked += StatisticsButtonClickedEventHandler;

            _creditsButton = DisplayedDocument.rootVisualElement.Q<Button>("Credits-button");
            _creditsButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _creditsButton.clicked += CreditsButtonClickedEventHandler;

            _cheatsButton = DisplayedDocument.rootVisualElement.Q<Button>("Cheats-button");
            _cheatsButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _cheatsButton.clicked += CheatsButtonClickedEventHandler;

            var operation = LocalizationSettings.InitializationOperation;
            await UniTask.WaitUntil(() => operation.IsDone);

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

            _playButton.UnregisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _playButton.clicked -= PlayButtonClickedEventHandler;
            _playButton = null;

            _inventoryButton.UnregisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _inventoryButton.clicked -= InventoryButtonClickedEventHandler;
            _inventoryButton = null;

            _armoryButton.UnregisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _armoryButton.clicked -= ArmoryButtonClickedEventHandler;
            _armoryButton = null;

            _settingsButton.UnregisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _settingsButton.clicked -= SettingsButtonClickedEventHandler;
            _settingsButton = null;

            _statisticsButton.UnregisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _statisticsButton.clicked -= StatisticsButtonClickedEventHandler;
            _statisticsButton = null;

            _creditsButton.UnregisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _creditsButton.clicked -= CreditsButtonClickedEventHandler;
            _creditsButton = null;

            _cheatsButton.UnregisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _cheatsButton.clicked -= CheatsButtonClickedEventHandler;
            _cheatsButton = null;

            DisplayedDocument.visualTreeAsset = null;
            Enabled = false;
        }

        #region interfaces

        public void Initialize()
        {
            EnableAsync().Forget();

            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
        }

        public void Dispose()
        {
            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
        }

        #endregion

        #region event handlers

        private void PointerOverEventHandler(PointerOverEvent e)
        {
            UIAudio.HoverOverAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void PlayButtonClickedEventHandler()
        {
            Disable();

            _levelSelectionDisplay.Access.EnableAsync().Forget();
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void InventoryButtonClickedEventHandler()
        {
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void ArmoryButtonClickedEventHandler()
        {
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void SettingsButtonClickedEventHandler()
        {
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void StatisticsButtonClickedEventHandler()
        {
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void CreditsButtonClickedEventHandler()
        {
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void CheatsButtonClickedEventHandler()
        {
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            EnableAsync().Forget();
        }

        #endregion
    }
}