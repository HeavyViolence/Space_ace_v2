using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;
using System.Threading;

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
                                    UIAudio uIAudio,
                                    LevelDisplay levelDisplay,
                                    GamePauseDisplay gamePauseDisplay,
                                    GamePauser gamePauser,
                                    GameControlsTransmitter gameControlsTransmitter) : base(audioPlayer, uIAudio)
        {
            _levelDisplay = levelDisplay ?? throw new ArgumentNullException(nameof(levelDisplay),
                $"Attempted to pass an empty {typeof(LevelDisplay)}!");

            _gamePauseDisplay = gamePauseDisplay ?? throw new ArgumentNullException(nameof(GamePauseDisplay),
                $"Attempted to pass an empty {typeof(GamePauseDisplay)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");

            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException(nameof(gameControlsTransmitter),
                $"Attempted to pass an empty {typeof(GameControlsTransmitter)}!");
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
            AudioPlayer.PlayOnceAsync(UIAudio.BackwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        #endregion
    }
}