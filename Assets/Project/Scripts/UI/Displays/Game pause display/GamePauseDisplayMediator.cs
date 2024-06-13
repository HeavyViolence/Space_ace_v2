using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;

using UnityEngine;
using UnityEngine.UIElements;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.UI.Displays
{
    public sealed class GamePauseDisplayMediator : UIDisplayMediator
    {
        private readonly UISettings _settings;

        private readonly GamePauseDisplay _gamePauseDisplay;
        private readonly LevelDisplay _levelDisplay;
        private readonly MainMenuDisplay _mainMenuDisplay;
        private readonly ScreenFader _screenFader;

        private readonly GamePauser _gamePauser;
        private readonly GameStateLoader _gameStateLoader;
        private readonly GameControlsTransmitter _gameControlsTransmitter;

        public GamePauseDisplayMediator(UISettings settings,
                                        AudioPlayer audioPlayer,
                                        UIAudio uiAudio,
                                        GamePauseDisplay gamePauseDisplay,
                                        LevelDisplay levelDisplay,
                                        MainMenuDisplay mainMenuDisplay,
                                        ScreenFader screenFader,
                                        GamePauser gamePauser,
                                        GameStateLoader gameStateLoader,
                                        GameControlsTransmitter gameControlsTransmitter) : base(audioPlayer, uiAudio)
        {
            if (settings == null) throw new ArgumentNullException();
            _settings = settings;

            _gamePauseDisplay = gamePauseDisplay ?? throw new ArgumentNullException();
            _levelDisplay = levelDisplay ?? throw new ArgumentNullException();
            _mainMenuDisplay = mainMenuDisplay ?? throw new ArgumentNullException();
            _screenFader = screenFader ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException();
        }

        #region interfaces

        public override void Initialize()
        {
            _gamePauseDisplay.MainMenuButtonClicked += async (sender, args) => await MainMenuButtonClickedEventHandlerAsync(sender, args);
            _gamePauseDisplay.ResumeButtonClicked += ResumeButtonClickedEventHandler;

            _gamePauseDisplay.PointerOver += PointerOverEventHandler;

            _gameControlsTransmitter.GoToPreviousMenu += GoToPreviousMenuEventHandler;
        }

        public override void Dispose()
        {
            _gamePauseDisplay.MainMenuButtonClicked -= async (sender, args) => await MainMenuButtonClickedEventHandlerAsync(sender, args);
            _gamePauseDisplay.ResumeButtonClicked -= ResumeButtonClickedEventHandler;

            _gamePauseDisplay.PointerOver -= PointerOverEventHandler;

            _gameControlsTransmitter.GoToPreviousMenu -= GoToPreviousMenuEventHandler;
        }

        #endregion

        #region event handlers

        private async UniTask MainMenuButtonClickedEventHandlerAsync(object sender, EventArgs e)
        {
            _gamePauseDisplay.DisableAsync().Forget();
            _screenFader.FadeInAndOutAsync(_settings.FadingDuration).Forget();
            AudioPlayer.PlayOnceAsync(UIAudio.ForwardClick.Random, Vector3.zero).Forget();

            await _gameStateLoader.LoadMainMenuAsync(_settings.LoadingDelay);

            _mainMenuDisplay.EnableAsync().Forget();
            _gamePauser.Resume();
        }

        private void ResumeButtonClickedEventHandler(object sender, EventArgs e)
        {
            _gamePauseDisplay.DisableAsync().Forget();
            _levelDisplay.EnableAsync().Forget();
            _gamePauser.Resume();
            AudioPlayer.PlayOnceAsync(UIAudio.ForwardClick.Random, Vector3.zero).Forget();
        }

        private void PointerOverEventHandler(object sender, PointerOverEvent e)
        {
            AudioPlayer.PlayOnceAsync(UIAudio.HoverOver.Random, Vector3.zero).Forget();
        }

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (_gamePauseDisplay.Active == false) return;

            _gamePauseDisplay.DisableAsync().Forget();
            _levelDisplay.EnableAsync().Forget();
            _gamePauser.Resume();
            AudioPlayer.PlayOnceAsync(UIAudio.BackwardClick.Random, Vector3.zero).Forget();
        }

        #endregion
    }
}