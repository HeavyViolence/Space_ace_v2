using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;

using UnityEngine;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.UI
{
    public sealed class LevelDisplayMediator : UIDisplayMediator
    {
        private readonly LevelDisplay _levelDisplay;
        private readonly GamePauseDisplay _gamePauseDisplay;

        private readonly GamePauser _gamePauser;
        private readonly GameControlsTransmitter _gameControlsTransmitter;

        public LevelDisplayMediator(AudioPlayer audioPlayer,
                                    UIAudio uiAudio,
                                    LevelDisplay levelDisplay,
                                    GamePauseDisplay gamePauseDisplay,
                                    GamePauser gamePauser,
                                    GameControlsTransmitter gameControlsTransmitter) : base(audioPlayer, uiAudio)
        {
            _levelDisplay = levelDisplay ?? throw new ArgumentNullException();
            _gamePauseDisplay = gamePauseDisplay ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException();
        }

        #region interfaces

        public override void Initialize()
        {
            _gameControlsTransmitter.GoToPreviousMenu += GoToPreviousMenuEventHandler;
        }

        public override void Dispose()
        {
            _gameControlsTransmitter.GoToPreviousMenu -= GoToPreviousMenuEventHandler;
        }

        #endregion

        #region event handlers

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (_levelDisplay.Enabled == false) return;

            _levelDisplay.Disable();
            _gamePauseDisplay.EnableAsync().Forget();
            _gamePauser.Pause();
            AudioPlayer.PlayOnceAsync(UIAudio.BackwardClick.Random, Vector3.zero).Forget();
        }

        #endregion
    }
}