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
                                        UIAudio uIAudio,
                                        GamePauseDisplay gamePauseDisplay,
                                        LevelDisplay levelDisplay,
                                        MainMenuDisplay mainMenuDisplay,
                                        ScreenFader screenFader,
                                        GamePauser gamePauser,
                                        GameStateLoader gameStateLoader,
                                        GameControlsTransmitter gameControlsTransmitter) : base(audioPlayer, uIAudio)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings),
                    $"Attempted to pass an empty {typeof(UISettings)}!");

            _settings = settings;

            _gamePauseDisplay = gamePauseDisplay ?? throw new ArgumentNullException(nameof(gamePauseDisplay),
                $"Attempted to pass an empty {typeof(GamePauseDisplay)}!");

            _levelDisplay = levelDisplay ?? throw new ArgumentNullException(nameof(levelDisplay),
                $"Attempted to pass an empty {typeof(LevelDisplay)}!");

            _mainMenuDisplay = mainMenuDisplay ?? throw new ArgumentNullException(nameof(mainMenuDisplay),
                $"Attempte dto pass an empty {typeof(MainMenuDisplay)}!");

            _screenFader = screenFader ?? throw new ArgumentNullException(nameof(screenFader),
                $"Attempted to pass an empty {typeof(ScreenFader)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException(nameof(gameControlsTransmitter),
                $"Attempted to pass an empty {typeof(GameControlsTransmitter)}!");
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
            _gamePauseDisplay.Disable();
            _screenFader.FadeInAndOutAsync(_settings.FadingDuration).Forget();
            AudioPlayer.PlayOnceAsync(UIAudio.ForwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();

            await _gameStateLoader.LoadMainMenuAsync(_settings.LoadingDelay);

            _mainMenuDisplay.EnableAsync().Forget();
            _gamePauser.Resume();
        }

        private void ResumeButtonClickedEventHandler(object sender, EventArgs e)
        {
            _gamePauseDisplay.Disable();
            _levelDisplay.EnableAsync().Forget();
            _gamePauser.Resume();
            AudioPlayer.PlayOnceAsync(UIAudio.ForwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        private void PointerOverEventHandler(object sender, PointerOverEvent e)
        {
            AudioPlayer.PlayOnceAsync(UIAudio.HoverOver.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (_gamePauseDisplay.Enabled == false) return;

            _gamePauseDisplay.Disable();
            _levelDisplay.EnableAsync().Forget();
            _gamePauser.Resume();
            AudioPlayer.PlayOnceAsync(UIAudio.BackwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        #endregion
    }
}