using Cysharp.Threading.Tasks;
using SpaceAce.Architecture;
using SpaceAce.Main.Localization;
using SpaceAce.Main.Audio;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

namespace SpaceAce.UI
{
    public sealed class MainMenuDisplay : UIDisplay, IInitializable
    {
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
                               UIAudio audio,
                               Localizer localizer) : base(displayAsset, settings, audio, localizer) { }

        public override async UniTaskVoid EnableAsync()
        {
            if (Enabled) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;

            _playButton = DisplayedDocument.rootVisualElement.Q<Button>("Play-button");
            _playButton.RegisterCallback<MouseOverEvent>(OnMouseOver);
            _playButton.clicked += PlayButtonClickedEventHandler;

            _inventoryButton = DisplayedDocument.rootVisualElement.Q<Button>("Inventory-button");
            _inventoryButton.RegisterCallback<MouseOverEvent>(OnMouseOver);
            _inventoryButton.clicked += InventoryButtonClickedEventHandler;

            _armoryButton = DisplayedDocument.rootVisualElement.Q<Button>("Armory-button");
            _armoryButton.RegisterCallback<MouseOverEvent>(OnMouseOver);
            _armoryButton.clicked += ArmoryButtonClickedEventHandler;

            _settingsButton = DisplayedDocument.rootVisualElement.Q<Button>("Settings-button");
            _settingsButton.RegisterCallback<MouseOverEvent>(OnMouseOver);
            _settingsButton.clicked += SettingsButtonClickedEventHandler;

            _statisticsButton = DisplayedDocument.rootVisualElement.Q<Button>("Statistics-button");
            _statisticsButton.RegisterCallback<MouseOverEvent>(OnMouseOver);
            _statisticsButton.clicked += StatisticsButtonClickedEventHandler;

            _creditsButton = DisplayedDocument.rootVisualElement.Q<Button>("Credits-button");
            _creditsButton.RegisterCallback<MouseOverEvent>(OnMouseOver);
            _creditsButton.clicked += CreditsButtonClickedEventHandler;

            _cheatsButton = DisplayedDocument.rootVisualElement.Q<Button>("Cheats-button");
            _cheatsButton.RegisterCallback<MouseOverEvent>(OnMouseOver);
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

            Enabled = true;
        }

        public override void Disable()
        {
            if (Enabled == false) return;

            _playButton.clicked -= PlayButtonClickedEventHandler;
            _playButton = null;

            _inventoryButton.clicked -= InventoryButtonClickedEventHandler;
            _inventoryButton = null;

            _armoryButton.clicked -= ArmoryButtonClickedEventHandler;
            _armoryButton = null;

            _settingsButton.clicked -= SettingsButtonClickedEventHandler;
            _settingsButton = null;

            _statisticsButton.clicked -= StatisticsButtonClickedEventHandler;
            _statisticsButton = null;

            _creditsButton.clicked -= CreditsButtonClickedEventHandler;
            _creditsButton = null;

            _cheatsButton.clicked -= CheatsButtonClickedEventHandler;
            _cheatsButton = null;

            DisplayedDocument.visualTreeAsset = null;
            Enabled = false;
        }

        #region interfaces

        public void Initialize()
        {
            EnableAsync().Forget();
        }

        #endregion

        #region event handlers

        private void OnMouseOver(MouseOverEvent e)
        {
            UIAudio.HoverOverAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void PlayButtonClickedEventHandler()
        {
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

        #endregion
    }
}