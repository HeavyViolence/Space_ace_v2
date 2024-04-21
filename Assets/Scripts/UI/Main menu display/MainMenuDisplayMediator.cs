using Cysharp.Threading.Tasks;

using SpaceAce.Main.Audio;

using System;

using UnityEngine;

namespace SpaceAce.UI
{
    public sealed class MainMenuDisplayMediator : UIDisplayMediator
    {
        private readonly MainMenuDisplay _mainMenuDisplay;
        private readonly LevelSelectionDisplay _levelSelectionDisplay;

        public MainMenuDisplayMediator(AudioPlayer audioPlayer,
                                       UIAudio uiAudio,
                                       MainMenuDisplay mainMenuDisplay,
                                       LevelSelectionDisplay levelSelectionDisplay) : base(audioPlayer, uiAudio)
        {
            _mainMenuDisplay = mainMenuDisplay ?? throw new ArgumentNullException();
            _levelSelectionDisplay = levelSelectionDisplay ?? throw new ArgumentNullException();
        }

        #region interfaces

        public override void Initialize()
        {
            _mainMenuDisplay.EnableAsync().Forget();

            _mainMenuDisplay.PlayButtonClicked += PlayButtonClickedEventHandler;
            _mainMenuDisplay.PointerOver += PointerOverEventHandler;
        }

        public override void Dispose()
        {
            _mainMenuDisplay.PlayButtonClicked -= PlayButtonClickedEventHandler;
            _mainMenuDisplay.PointerOver -= PointerOverEventHandler;
        }

        #endregion

        #region event handlers

        private void PlayButtonClickedEventHandler(object sender, EventArgs e)
        {
            _mainMenuDisplay.DisableAsync().Forget();
            _levelSelectionDisplay.EnableAsync().Forget();
            AudioPlayer.PlayOnceAsync(UIAudio.ForwardClick.Random, Vector3.zero).Forget();
        }

        private void PointerOverEventHandler(object sender, UnityEngine.UIElements.PointerOverEvent e)
        {
            AudioPlayer.PlayOnceAsync(UIAudio.HoverOver.Random, Vector3.zero).Forget();
        }

        #endregion
    }
}