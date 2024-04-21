using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;

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
            if (settings == null) throw new ArgumentNullException();
            _settings = settings;

            _levelSelectionDisplay = levelSelectionDisplay ?? throw new ArgumentNullException();
            _mainMenuDisplay = mainMenuDisplay ?? throw new ArgumentNullException();
            _levelDisplay = levelDisplay ?? throw new ArgumentNullException();
            _screenFader = screenFader ?? throw new ArgumentNullException();
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException();
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
            _levelSelectionDisplay.DisableAsync().Forget();
            _mainMenuDisplay.EnableAsync().Forget();
            AudioPlayer.PlayOnceAsync(UIAudio.BackwardClick.Random, Vector3.zero).Forget();
        }

        private async UniTask BattleButtonClickedEventHandlerAsync(object sender, EventArgs e)
        {
            _levelSelectionDisplay.DisableAsync().Forget();
            _screenFader.FadeInAndOutAsync(_settings.FadingDuration).Forget();
            AudioPlayer.PlayOnceAsync(UIAudio.ForwardClick.Random, Vector3.zero).Forget();

            await _gameStateLoader.LoadLevelAsync(_selectedLevelIndex, _settings.LoadingDelay);

            _levelDisplay.EnableAsync().Forget();
        }

        private void LevelButtonCheckedEventHandler(object sender, LevelButtonCheckedEventArgs e)
        {
            _selectedLevelIndex = e.LevelIndex;
            AudioPlayer.PlayOnceAsync(UIAudio.ForwardClick.Random, Vector3.zero).Forget();
        }

        private void LevelButtonUncheckedEventHandler(object sender, EventArgs e)
        {
            _selectedLevelIndex = 0;
        }

        private void PointerOverEventHandler(object sender, PointerOverEvent e)
        {
            AudioPlayer.PlayOnceAsync(UIAudio.HoverOver.Random, Vector3.zero).Forget();
        }

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (_levelSelectionDisplay.Active == false) return;

            _levelSelectionDisplay.DisableAsync().Forget();
            _mainMenuDisplay.EnableAsync().Forget();
            AudioPlayer.PlayOnceAsync(UIAudio.BackwardClick.Random, Vector3.zero).Forget();
        }

        #endregion
    }
}