using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Levels;
using SpaceAce.Main.Localization;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public sealed class LevelSelectionDisplay : UIDisplay
    {
        private const float BattleButtonLockDelay = 0.1f;

        public event EventHandler MainMenuButtonClicked;
        public event EventHandler BattleButtonClicked;
        public event EventHandler<LevelButtonCheckedEventArgs> LevelButtonChecked;
        public event EventHandler LevelButtonUnchecked;
        public event EventHandler<PointerOverEvent> PointerOver;

        protected override string DisplayHolderName => "Level selection display";

        private readonly VisualTreeAsset _levelButtonAsset;
        private readonly LevelUnlocker _levelUnlocker;
        private readonly BestLevelRunStatisticsCollector _bestLevelsRunStatisticsCollector;

        private readonly List<Button> _levelsButtons = new();

        private Label _damageAvertedLabel;
        private VisualElement _damageAvertedBar;

        private Label _damageDealtLabel;
        private VisualElement _damageDealtBar;

        private Label _accuracyLabel;
        private VisualElement _accuracyBar;

        private Label _meteorsDestroyedLabel;
        private VisualElement _meteorsDestroyedBar;

        private Label _wrecksDestroyedLabel;
        private VisualElement _wrecksDestroyedBar;

        private Label _experienceEarnedLabel;
        private VisualElement _experienceEarnedBar;

        private Label _levelMasteryLabel;
        private VisualElement _levelMasteryBar;

        private Label _enemiesDefeatedCounter;
        private Label _meteorsDestroyedCounter;
        private Label _wrecksDestroyedCounter;
        private Label _creditsEarnedCounter;
        private Label _experienceEarnedCounter;
        private Label _levelRunTimeCounter;

        private Button _mainMenuButton;
        private Button _battleButton;

        private VisualElement _levelsButtonsAnchor;

        private int _selectedLevelIndexToPlay = 0;

        public LevelSelectionDisplay(LevelSelectionDisplayAssets assets,
                                     Localizer localizer,
                                     LevelUnlocker levelUnlocker,
                                     BestLevelRunStatisticsCollector bestLevelRunStatisticsCollector) : base(assets.Display,
                                                                                                             assets.Settings,
                                                                                                             localizer)
        {
            if (assets.LevelButton == null) throw new ArgumentNullException();

            _levelButtonAsset = assets.LevelButton;

            _levelUnlocker = levelUnlocker ?? throw new ArgumentNullException();
            _bestLevelsRunStatisticsCollector = bestLevelRunStatisticsCollector ?? throw new ArgumentNullException();
        }

        public override async UniTask EnableAsync()
        {
            if (Active == true) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;
            DisplayedDocument.rootVisualElement.style.opacity = 0;

            _selectedLevelIndexToPlay = 0;

            Font localizedFont = await Localizer.GetLocalizedFontAsync();

            await UpdateHeadlineAsync(localizedFont);
            await UpdateButtonsAsync(localizedFont);
            await UpdateBestLevelRunStatisticsDisplayAsync(0, localizedFont);

            DisplayedDocument.rootVisualElement.style.opacity = 1;
            Active = true;
        }

        public override async UniTask DisableAsync()
        {
            if (Active == false) return;

            DisplayedDocument.visualTreeAsset = null;

            ClearButtons();
            ClearBestLevelRunStatisticsDisplay();

            await base.DisableAsync();
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

            AddButtonsForUnlockedLevels(font);
        }

        private async UniTask UpdateMainMenuButtonAsync(Font font)
        {
            _mainMenuButton = DisplayedDocument.rootVisualElement.Q<Button>("Main-menu-button");

            string mainMenuButtonLocalizedText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Main menu button text");

            _mainMenuButton.style.unityFont = font;
            _mainMenuButton.text = mainMenuButtonLocalizedText;

            _mainMenuButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _mainMenuButton.clicked += MainMenuButtonClickedEventHandler;
        }

        private async UniTask UpdateBattleButtonAsync(Font font)
        {
            _battleButton = DisplayedDocument.rootVisualElement.Q<Button>("Battle-button");

            if (_selectedLevelIndexToPlay == 0) _battleButton.SetEnabled(false);

            string battleButtonLocalizedText = await Localizer.GetLocalizedStringAsync("Level selection menu", "Battle button text");

            _battleButton.style.unityFont = font;
            _battleButton.text = battleButtonLocalizedText;

            _battleButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _battleButton.clicked += BattleButtonClickedEventHandler;
        }

        private void AddButtonsForUnlockedLevels(Font font)
        {
            _levelsButtonsAnchor ??= DisplayedDocument.rootVisualElement.Q<VisualElement>("Levels-buttons-anchor");

            for (int levelIndex = 1; levelIndex <= _levelUnlocker.LargestUnlockedLevelIndex; levelIndex++)
            {
                VisualElement levelButtonTreeRoot = _levelButtonAsset.CloneTree();

                Button levelButton = levelButtonTreeRoot.Q<Button>($"Level-button");
                levelButton.name = $"Level-{levelIndex}-button";

                levelButton.style.unityFont = font;
                levelButton.text = levelIndex.ToString();

                levelButton.clicked += () => LevelButtonCheckedEventHandler(levelButton, font);
                levelButton.RegisterCallback<FocusOutEvent>(async (e) => await LevelButtonUncheckedEventHandlerAsync(font));
                levelButton.RegisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));

                _levelsButtons.Add(levelButton);
                _levelsButtonsAnchor.Add(levelButtonTreeRoot);
            }
        }

        private void ClearButtons()
        {
            _mainMenuButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _mainMenuButton.clicked -= MainMenuButtonClickedEventHandler;
            _mainMenuButton = null;

            _battleButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            _battleButton.clicked -= BattleButtonClickedEventHandler;
            _battleButton = null;

            foreach (Button levelButton in _levelsButtons)
            {
                levelButton.clicked -= () => LevelButtonCheckedEventHandler(levelButton, null);
                levelButton.UnregisterCallback<FocusOutEvent>(async (e) => await LevelButtonUncheckedEventHandlerAsync(null));
                levelButton.UnregisterCallback<PointerOverEvent>((e) => PointerOverEventHandler(e));
            }

            _levelsButtons.Clear();
            _levelsButtonsAnchor = null;
        }

        private async UniTask UpdateBestLevelRunStatisticsDisplayAsync(int levelIndex, Font font)
        {
            if (font == null || DisplayedDocument.visualTreeAsset == null) return;

            BestLevelRunStatistics statistics = _bestLevelsRunStatisticsCollector.GetStatistics(levelIndex);

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
            _levelRunTimeCounter.text = $"{statistics.RunTime.TotalMinutes}:{statistics.RunTime.Seconds}.{statistics.RunTime.Milliseconds}";
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

        #region event handlers

        private void PointerOverEventHandler(PointerOverEvent e)
        {
            PointerOver?.Invoke(this, e);
        }

        private void MainMenuButtonClickedEventHandler()
        {
            MainMenuButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BattleButtonClickedEventHandler()
        {
            BattleButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void LevelButtonCheckedEventHandler(Button button, Font font)
        {
            string buttonIndex = button.name.Split('-')[1];
            int levelIndex = Convert.ToInt32(buttonIndex);

            _selectedLevelIndexToPlay = levelIndex;
            _battleButton.SetEnabled(true);

            LevelButtonChecked?.Invoke(this, new(levelIndex));

            UpdateBestLevelRunStatisticsDisplayAsync(levelIndex, font).Forget();
        }

        private async UniTask LevelButtonUncheckedEventHandlerAsync(Font font)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(BattleButtonLockDelay));

            _selectedLevelIndexToPlay = 0;
            _battleButton?.SetEnabled(false);

            LevelButtonUnchecked?.Invoke(this, EventArgs.Empty);

            await UpdateBestLevelRunStatisticsDisplayAsync(0, font);
        }

        #endregion
    }
}