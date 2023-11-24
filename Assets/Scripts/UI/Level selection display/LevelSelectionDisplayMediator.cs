using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;
using System.Threading;

using UnityEngine;
using UnityEngine.UIElements;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.UI
{
    public sealed class LevelSelectionDisplayMediator : UIDisplayMediator
    {
        private readonly UISettings _settings;

        private readonly LevelSelectionDisplay _levelSelectionDisplay;
        private readonly MainMenuDisplay _mainMenuDisplay;
        private readonly LevelDisplay _levelDisplay;
        private readonly ScreenFader _screenFader;

        private readonly GameStateLoader _gameStateLoader;
        private readonly GameControlsTransmitter _gameControlsTransmitter;

        private int _selectedLevelIndex = 0;

        public LevelSelectionDisplayMediator(UISettings settings,
                                             AudioPlayer audioPlayer,
                                             UIAudio uiAudio,
                                             LevelSelectionDisplay levelSelectionDisplay,
                                             MainMenuDisplay mainMenuDisplay,
                                             LevelDisplay levelDisplay,
                                             ScreenFader screenFader,
                                             GameStateLoader gameStateLoader,
                                             GameControlsTransmitter gameControlsTransmitter) : base(audioPlayer, uiAudio)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings),
                    $"Attempted to pass an empty {typeof(UISettings)}!");

            _settings = settings;

            _levelSelectionDisplay = levelSelectionDisplay ?? throw new ArgumentNullException(nameof(levelSelectionDisplay),
                $"Attempted to pass an empty {typeof(LevelSelectionDisplay)}!");

            _mainMenuDisplay = mainMenuDisplay ?? throw new ArgumentNullException(nameof(mainMenuDisplay),
                $"Attempted to pass an empty {typeof(MainMenuDisplay)}!");

            _levelDisplay = levelDisplay ?? throw new ArgumentNullException(nameof(levelDisplay),
                $"Attempted to pass an empty {typeof(LevelDisplay)}!");

            _screenFader = screenFader ?? throw new ArgumentNullException(nameof(screenFader),
                $"Attempted to pass an empty {typeof(ScreenFader)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException(nameof(gameControlsTransmitter),
                $"Attempted to pass an empty {typeof(GameControlsTransmitter)}!");
        }

        #region interfaces

        public override void Initialize()
        {
            _levelSelectionDisplay.MainMenuButtonClicked += MainMenuButtonClickedEventHandler;

            _levelSelectionDisplay.BattleButtonClicked += async (sender, args) => await BattleButtonClickedEventHandlerAsync(sender, args);

            _levelSelectionDisplay.LevelButtonChecked += LevelButtonCheckedEventHandler;
            _levelSelectionDisplay.LevelButtonUnchecked += LevelButtonUncheckedEventHandler;

            _levelSelectionDisplay.PointerOver += PointerOverEventHandler;

            _gameControlsTransmitter.GoToPreviousMenu += GoToPreviousMenuEventHandler;
        }

        public override void Dispose()
        {
            _levelSelectionDisplay.MainMenuButtonClicked -= MainMenuButtonClickedEventHandler;

            _levelSelectionDisplay.BattleButtonClicked -= async (sender, args) => await BattleButtonClickedEventHandlerAsync(sender, args);

            _levelSelectionDisplay.LevelButtonChecked -= LevelButtonCheckedEventHandler;
            _levelSelectionDisplay.LevelButtonUnchecked -= LevelButtonUncheckedEventHandler;

            _levelSelectionDisplay.PointerOver -= PointerOverEventHandler;

            _gameControlsTransmitter.GoToPreviousMenu -= GoToPreviousMenuEventHandler;
        }

        #endregion

        #region event handlers

        private void MainMenuButtonClickedEventHandler(object sender, EventArgs e)
        {
            _levelSelectionDisplay.Disable();
            _mainMenuDisplay.EnableAsync().Forget();
            AudioPlayer.PlayOnceAsync(UIAudio.BackwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        private async UniTask BattleButtonClickedEventHandlerAsync(object sender, EventArgs e)
        {
            _levelSelectionDisplay.Disable();
            _screenFader.FadeInAndOutAsync(_settings.FadingDuration).Forget();
            AudioPlayer.PlayOnceAsync(UIAudio.ForwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();

            await _gameStateLoader.LoadLevelAsync(_selectedLevelIndex, _settings.LoadingDelay);

            _levelDisplay.EnableAsync().Forget();
        }

        private void LevelButtonCheckedEventHandler(object sender, LevelButtonCheckedEventArgs e)
        {
            _selectedLevelIndex = e.LevelIndex;
            AudioPlayer.PlayOnceAsync(UIAudio.ForwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        private void LevelButtonUncheckedEventHandler(object sender, EventArgs e)
        {
            _selectedLevelIndex = 0;
        }

        private void PointerOverEventHandler(object sender, PointerOverEvent e)
        {
            AudioPlayer.PlayOnceAsync(UIAudio.HoverOver.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (_levelSelectionDisplay.Enabled == false) return;

            _levelSelectionDisplay.Disable();
            _mainMenuDisplay.EnableAsync().Forget();
            AudioPlayer.PlayOnceAsync(UIAudio.BackwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        #endregion
    }
}