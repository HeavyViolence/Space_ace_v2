using Cysharp.Threading.Tasks;

using SpaceAce.Architecture;
using SpaceAce.Gameplay.Controls;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Localization;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.UI
{
    public sealed class LevelSelectionDisplay : UIDisplay, IInitializable, IDisposable
    {
        private const float BattleButtonLockDelay = 0.1f;

        protected override string DisplayHolderName => "Level selection display";

        private readonly VisualTreeAsset _levelButtonAsset = null;
        private readonly GameStateLoader _gameStateLoader = null;
        private readonly LevelsUnlocker _levelsUnlocker = null;
        private readonly BestLevelsRunsStatisticsCollector _bestLevelsRunsStatisticsCollector = null;
        private readonly GameControlsTransmitter _gameControlsTransmitter = null;

        private readonly CachedService<MainMenuDisplay> _mainMenuDisplay = new();
        private readonly List<Button> _levelsButtons = new();

        private Label _damageAvertedLabel = null;
        private VisualElement _damageAvertedBar = null;

        private Label _damageDealtLabel = null;
        private VisualElement _damageDealtBar = null;

        private Label _accuracyLabel = null;
        private VisualElement _accuracyBar = null;

        private Label _meteorsDestroyedLabel = null;
        private VisualElement _meteorsDestroyedBar = null;

        private Label _wrecksDestroyedLabel = null;
        private VisualElement _wrecksDestroyedBar = null;

        private Label _experienceEarnedLabel = null;
        private VisualElement _experienceEarnedBar = null;

        private Label _levelMasteryLabel = null;
        private VisualElement _levelMasteryBar = null;

        private Label _enemiesDefeatedCounter = null;
        private Label _meteorsDestroyedCounter = null;
        private Label _wrecksDestroyedCounter = null;
        private Label _creditsEarnedCounter = null;
        private Label _experienceEarnedCounter = null;
        private Label _levelRunTimeCounter = null;

        private Button _mainMenuButton = null;
        private Button _battleButton = null;

        private VisualElement _levelsButtonsAnchor = null;

        private int _selectedLevelIndexToPlay = 0;                

        public LevelSelectionDisplay(VisualTreeAsset displayAsset,
                                     VisualTreeAsset levelButtonAsset,
                                     PanelSettings settings,
                                     UIAudio audio,
                                     Localizer localizer,
                                     GameStateLoader gameStateLoader,
                                     LevelsUnlocker levelsUnlocker,
                                     BestLevelsRunsStatisticsCollector bestLevelsRunsStatisticsCollector,
                                     GameControlsTransmitter gameControlsTransmitter) : base(displayAsset, settings, audio, localizer)
        {
            if (levelButtonAsset == null)
                throw new ArgumentNullException(nameof(levelButtonAsset),
                    $"Attempted to pass an empty level button asset: {typeof(VisualTreeAsset)}!");

            _levelButtonAsset = levelButtonAsset;

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _levelsUnlocker = levelsUnlocker ?? throw new ArgumentNullException(nameof(levelsUnlocker),
                $"Attempted to pass an empty {typeof(LevelsUnlocker)}!");

            _bestLevelsRunsStatisticsCollector = bestLevelsRunsStatisticsCollector ?? throw new ArgumentNullException(nameof(bestLevelsRunsStatisticsCollector),
                $"Attempted to pass an empty {typeof(BestLevelsRunsStatisticsCollector)}!");

            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException(nameof(gameControlsTransmitter),
                $"Attempted to pass an empty {typeof(GameControlsTransmitter)}!");
        }

        public override async UniTask EnableAsync()
        {
            if (Enabled == true) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;
            DisplayedDocument.rootVisualElement.style.opacity = 0;

            _selectedLevelIndexToPlay = 0;

            Font localizedFont = await Localizer.GetLocalizedFontAsync();

            await UpdateHeadlineAsync(localizedFont);
            await UpdateButtonsAsync(localizedFont);
            await UpdateBestLevelRunStatisticsDisplayAsync(0, localizedFont);

            DisplayedDocument.rootVisualElement.style.opacity = 1;
            Enabled = true;
        }

        public override void Disable()
        {
            if (Enabled == false) return;

            DisplayedDocument.visualTreeAsset = null;

            ClearButtons();
            ClearBestLevelRunStatisticsDisplay();

            Enabled = false;
        }

        private async UniTask UpdateHeadlineAsync(Font font)
        {
            Label headline = DisplayedDocument.rootVisualElement.Q<Label>("Headline-label");

            string localizedText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Headline text");

            headline.style.unityFont = font;
            headline.text = localizedText;
        }

        private async UniTask UpdateButtonsAsync(Font font)
        {
            await UpdateMainMenuButtonAsync(font);
            await UpdateBattleButtonAsync(font);

            AddSelectionButtonsForUnlockedLevels(font);
        }

        private async UniTask UpdateMainMenuButtonAsync(Font font)
        {
            _mainMenuButton = DisplayedDocument.rootVisualElement.Q<Button>("Main-menu-button");

            string mainMenuButtonLocalizedText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Main menu button text");

            _mainMenuButton.style.unityFont = font;
            _mainMenuButton.text = mainMenuButtonLocalizedText;

            _mainMenuButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _mainMenuButton.clicked += MainMenuButtonClickedEventHandler;
        }

        private async UniTask UpdateBattleButtonAsync(Font font)
        {
            _battleButton = DisplayedDocument.rootVisualElement.Q<Button>("Battle-button");

            if (_selectedLevelIndexToPlay == 0) _battleButton.SetEnabled(false);

            string battleButtonLocalizedText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Battle button text");

            _battleButton.style.unityFont = font;
            _battleButton.text = battleButtonLocalizedText;

            _battleButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _battleButton.clicked += BattleButtonClickedEventHandler;
        }

        private void AddSelectionButtonsForUnlockedLevels(Font font)
        {
            _levelsButtonsAnchor ??= DisplayedDocument.rootVisualElement.Q<VisualElement>("Levels-buttons-anchor");

            for (int levelIndex = 1; levelIndex <= _levelsUnlocker.LargestUnlockedLevelIndex; levelIndex++)
            {
                VisualElement levelButtonTreeRoot = _levelButtonAsset.CloneTree();

                Button levelButton = levelButtonTreeRoot.Q<Button>($"Level-button");
                levelButton.name = $"Level-{levelIndex}-button";

                levelButton.style.unityFont = font;
                levelButton.text = levelIndex.ToString();

                levelButton.clicked += async () => await LevelButtonCheckedEventHandlerAsync(levelButton, font);
                levelButton.RegisterCallback<FocusOutEvent>(async (e) => await LevelButtonUncheckedEventHandlerAsync(font));

                _levelsButtons.Add(levelButton);
                _levelsButtonsAnchor.Add(levelButtonTreeRoot);
            }
        }

        private void ClearButtons()
        {
            _mainMenuButton.UnregisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _mainMenuButton.clicked -= MainMenuButtonClickedEventHandler;
            _mainMenuButton = null;

            _battleButton.RegisterCallback<PointerOverEvent>(PointerOverEventHandler);
            _battleButton.clicked -= BattleButtonClickedEventHandler;
            _battleButton = null;

            foreach (Button levelButton in _levelsButtons)
            {
                levelButton.clicked -= async () => await LevelButtonCheckedEventHandlerAsync(levelButton, null);
                levelButton.UnregisterCallback<FocusOutEvent>(async (e) => await LevelButtonUncheckedEventHandlerAsync(null));
            }

            _levelsButtons.Clear();
            _levelsButtonsAnchor = null;
        }

        private async UniTask UpdateBestLevelRunStatisticsDisplayAsync(int levelIndex, Font font)
        {
            if (font == null || DisplayedDocument.visualTreeAsset == null) return;

            BestLevelRunStatistics statistics = _bestLevelsRunsStatisticsCollector.GetStatistics(levelIndex);

            string localizedDamageAvertedLabeltext = await Localizer.GetLocalizedStringAsync("Level selection menu", "Damage averted text", statistics);

            _damageAvertedLabel ??= DisplayedDocument.rootVisualElement.Q<Label>("Damage-averted-label");
            _damageAvertedLabel.style.unityFont = font;
            _damageAvertedLabel.text = localizedDamageAvertedLabeltext;

            _damageAvertedBar ??= DisplayedDocument.rootVisualElement.Q<VisualElement>("Damage-averted-bar-foreground");
            _damageAvertedBar.style.width = new(Length.Percent(statistics.DamageAvertedPercentage));

            string localizedDamageDealtLabeltext = await Localizer.GetLocalizedStringAsync("Level selection menu", "Damage dealt text", statistics);

            _damageDealtLabel ??= DisplayedDocument.rootVisualElement.Q<Label>("Damage-dealt-label");
            _damageDealtLabel.style.unityFont = font;
            _damageDealtLabel.text = localizedDamageDealtLabeltext;

            _damageDealtBar ??= DisplayedDocument.rootVisualElement.Q<VisualElement>("Damage-dealt-bar-foreground");
            _damageDealtBar.style.width = new(Length.Percent(statistics.DamageDealtPercentage));

            string localizedAccuracyLabelText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Accuracy text", statistics);

            _accuracyLabel ??= DisplayedDocument.rootVisualElement.Q<Label>("Accuracy-label");
            _accuracyLabel.style.unityFont = font;
            _accuracyLabel.text = localizedAccuracyLabelText;

            _accuracyBar ??= DisplayedDocument.rootVisualElement.Q<VisualElement>("Accuracy-bar-foreground");
            _accuracyBar.style.width = new(Length.Percent(statistics.AccuracyPercentage));

            string localizedMeteorsDestroyedLabelText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Meteors destroyed text", statistics);

            _meteorsDestroyedLabel ??= DisplayedDocument.rootVisualElement.Q<Label>("Meteors-destroyed-label");
            _meteorsDestroyedLabel.style.unityFont = font;
            _meteorsDestroyedLabel.text = localizedMeteorsDestroyedLabelText;

            _meteorsDestroyedBar ??= DisplayedDocument.rootVisualElement.Q<VisualElement>("Meteors-destroyed-bar-foreground");
            _meteorsDestroyedBar.style.width = new(Length.Percent(statistics.MeteorsDestroyedPercentage));

            string localizedWrecksDestroyedLabelText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Wrecks destroyed text", statistics);

            _wrecksDestroyedLabel ??= DisplayedDocument.rootVisualElement.Q<Label>("Wrecks-destroyed-label");
            _wrecksDestroyedLabel.style.unityFont = font;
            _wrecksDestroyedLabel.text = localizedWrecksDestroyedLabelText;

            _wrecksDestroyedBar ??= DisplayedDocument.rootVisualElement.Q<VisualElement>("Wrecks-destroyed-bar-foreground");
            _wrecksDestroyedBar.style.width = new(Length.Percent(statistics.WrecksDestroyedPercentage));

            string localizedExperienceEarnedLabelText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Experience earned text", statistics);

            _experienceEarnedLabel ??= DisplayedDocument.rootVisualElement.Q<Label>("Experience-earned-label");
            _experienceEarnedLabel.style.unityFont = font;
            _experienceEarnedLabel.text = localizedExperienceEarnedLabelText;

            _experienceEarnedBar ??= DisplayedDocument.rootVisualElement.Q<VisualElement>("Experience-earned-bar-foreground");
            _experienceEarnedBar.style.width = new(Length.Percent(statistics.ExperiencePercentage));

            string localizedLevelMasteryLabelText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Level mastery text", statistics);

            _levelMasteryLabel ??= DisplayedDocument.rootVisualElement.Q<Label>("Level-mastery-label");
            _levelMasteryLabel.style.unityFont = font;
            _levelMasteryLabel.text = localizedLevelMasteryLabelText;

            _levelMasteryBar ??= DisplayedDocument.rootVisualElement.Q<VisualElement>("Level-mastery-bar-foreground");
            _levelMasteryBar.style.width = new(Length.Percent(statistics.LevelMasteryPercentage));

            _enemiesDefeatedCounter ??= DisplayedDocument.rootVisualElement.Q<Label>("Enemies-defeated-counter");
            _enemiesDefeatedCounter.style.unityFont = font;
            _enemiesDefeatedCounter.text = statistics.EnemiesDefeated.ToString();

            _meteorsDestroyedCounter ??= DisplayedDocument.rootVisualElement.Q<Label>("Meteors-destroyed-counter");
            _meteorsDestroyedCounter.style.unityFont = font;
            _meteorsDestroyedCounter.text = $"{statistics.MeteorsDestroyed}/{statistics.MeteorsEncountered}";

            _wrecksDestroyedCounter ??= DisplayedDocument.rootVisualElement.Q<Label>("Wrecks-destroyed-counter");
            _wrecksDestroyedCounter.style.unityFont = font;
            _wrecksDestroyedCounter.text = $"{statistics.WrecksDestroyed}/{statistics.WrecksEncountered}";

            _creditsEarnedCounter ??= DisplayedDocument.rootVisualElement.Q<Label>("Credits-earned-counter");
            _creditsEarnedCounter.style.unityFont = font;
            _creditsEarnedCounter.text = $"{statistics.CreditsEarned:n}";

            _experienceEarnedCounter ??= DisplayedDocument.rootVisualElement.Q<Label>("Experience-earned-counter");
            _experienceEarnedCounter.style.unityFont = font;
            _experienceEarnedCounter.text = $"{statistics.ExperienceEarned:n}";

            _levelRunTimeCounter ??= DisplayedDocument.rootVisualElement.Q<Label>("Level-run-time-counter");
            _levelRunTimeCounter.style.unityFont = font;
            _levelRunTimeCounter.text = $"{statistics.RunTime.TotalMinutes}:{statistics.RunTime.Seconds}:{statistics.RunTime.Milliseconds}";
        }

        private void ClearBestLevelRunStatisticsDisplay()
        {
            _damageAvertedLabel = null;
            _damageAvertedBar = null;

            _damageDealtLabel = null;
            _damageDealtBar = null;

            _accuracyLabel = null;
            _accuracyBar = null;

            _meteorsDestroyedLabel = null;
            _meteorsDestroyedBar = null;

            _wrecksDestroyedLabel = null;
            _wrecksDestroyedBar = null;

            _experienceEarnedLabel = null;
            _experienceEarnedBar = null;

            _levelMasteryLabel = null;
            _levelMasteryBar = null;

            _enemiesDefeatedCounter = null;
            _meteorsDestroyedCounter = null;
            _wrecksDestroyedCounter = null;
            _creditsEarnedCounter = null;
            _experienceEarnedCounter = null;
            _levelRunTimeCounter = null;
        }

        #region interfaces

        public void Initialize()
        {
            _gameControlsTransmitter.GoToPreviousMenu += GoToPreviousMenuEventHandler;
        }

        public void Dispose()
        {
            _gameControlsTransmitter.GoToPreviousMenu -= GoToPreviousMenuEventHandler;
        }

        #endregion

        #region event handlers

        private void PointerOverEventHandler(PointerOverEvent e)
        {
            UIAudio.HoverOverAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void MainMenuButtonClickedEventHandler()
        {
            Disable();

            _mainMenuDisplay.Access.EnableAsync().Forget();
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private void BattleButtonClickedEventHandler()
        {
            Disable();

            _gameStateLoader.LoadLevelAsync(_selectedLevelIndexToPlay).Forget();
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
        }

        private async UniTask LevelButtonCheckedEventHandlerAsync(Button button, Font font)
        {
            UIAudio.ForwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);

            string buttonIndex = button.name.Split('-')[1];
            int levelIndex = Convert.ToInt32(buttonIndex);

            _selectedLevelIndexToPlay = levelIndex;
            _battleButton.SetEnabled(true);

            await UpdateBestLevelRunStatisticsDisplayAsync(levelIndex, font);
        }

        private async UniTask LevelButtonUncheckedEventHandlerAsync(Font font)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(BattleButtonLockDelay));

            UIAudio.BackwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
            _selectedLevelIndexToPlay = 0;
            _battleButton?.SetEnabled(false);
            
            await UpdateBestLevelRunStatisticsDisplayAsync(0, font);
        }

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (Enabled == true)
            {
                Disable();

                UIAudio.BackwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
                _mainMenuDisplay.Access.EnableAsync().Forget();
            }
        }

        #endregion
    }
}