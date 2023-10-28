using Cysharp.Threading.Tasks;

using SpaceAce.Architecture;
using SpaceAce.Gameplay.Controls;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Localization;

using System;

using UnityEngine;
using UnityEngine.UIElements;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.UI
{
    public sealed class LevelDisplay : UIDisplay, IInitializable, IDisposable
    {
        private readonly CachedService<GamePauseDisplay> _gamePauseDisplay = new();

        private readonly GameControlsTransmitter _gameControlsTransmitter = null;
        private readonly GameStateLoader _gameStateLoader = null;
        private readonly GamePauser _gamePauser = null;

        public LevelDisplay(VisualTreeAsset displayAsset,
                            PanelSettings settings,
                            UIAudio audio,
                            Localizer localizer,
                            GameControlsTransmitter gameControlsTransmitter,
                            GameStateLoader gameStateLoader,
                            GamePauser gamePauser) : base(displayAsset, settings, audio, localizer)
        {
            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException(nameof(gameControlsTransmitter),
                $"Attempted to pass an empty {typeof(GameControlsTransmitter)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");
        }

        protected override string DisplayHolderName => "Level display";

        public override async UniTask EnableAsync()
        {
            DisplayedDocument.visualTreeAsset = DisplayAsset;

            await UniTask.NextFrame();

            Enabled = true;
        }

        public override void Disable()
        {
            DisplayedDocument.visualTreeAsset = null;
            Enabled = false;
        }

        #region interfaces

        public void Initialize()
        {
            _gameControlsTransmitter.GoToPreviousMenu += GoToPreviousMenuEventHandler;

            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
        }

        public void Dispose()
        {
            _gameControlsTransmitter.GoToPreviousMenu -= GoToPreviousMenuEventHandler;

            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
        }

        #endregion

        #region event handlers

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (Enabled == true)
            {
                Disable();

                UIAudio.BackwardButtonClickAudio.PlayRandomAudioClip(Vector3.zero);
                _gamePauseDisplay.Access.EnableAsync().Forget();

                _gamePauser.Pause();
            }
        }

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            EnableAsync().Forget();
        }

        #endregion
    }
}