using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Controls;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.UI
{
    public sealed class UIMediator : IInitializable, IDisposable
    {
        private const float FadingDurationFactor = 2f;

        private readonly float _loadingDelay;

        private readonly UIAudio _uiAudio;
        private readonly IAudioPlayer _audioPlayer;

        private readonly IGameStateLoader _gameStateLoader;
        private readonly IGameControlsTransmitter _gameControlsTransmitter;
        private readonly IGamePauser _gamePauser;

        private readonly IMainMenuDisplay _mainMenuDisplay;
        private readonly ILevelSelectionDisplay _levelSelectionDisplay;
        private readonly IScreenFader _screenFader;
        private readonly ILevelDisplay _levelDisplay;
        private readonly IGamePauseDisplay _gamePauseDisplay;

        private int _selectedLevelIndex = 0;

        private float FadingDuration => _loadingDelay * FadingDurationFactor;

        public UIMediator(float loadingDelay,
                          UIAudio uiAudio,
                          IAudioPlayer audioPlayer,
                          IGameStateLoader gameStateLoader,
                          IGameControlsTransmitter gameControlsTransmitter,
                          IGamePauser gamePauser,
                          IMainMenuDisplay mainMenuDisplay,
                          ILevelSelectionDisplay levelSelectionDisplay,
                          IScreenFader screenFader,
                          ILevelDisplay levelDisplay,
                          IGamePauseDisplay gamePauseDisplay)
        {
            _loadingDelay = Mathf.Clamp(loadingDelay, IGameStateLoader.MinLoadingDelay, IGameStateLoader.MaxLoadingDelay);

            if (uiAudio == null)
                throw new ArgumentNullException(nameof(uiAudio),
                    $"Attempted to pass an empty {typeof(UIAudio)}!");

            _uiAudio = uiAudio;

            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer),
                $"Attempted to pass an empty {typeof(IAudioPlayer)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(IGameStateLoader)}!");

            _gameControlsTransmitter = gameControlsTransmitter ?? throw new ArgumentNullException(nameof(gameControlsTransmitter),
                $"Attempted to pass an empty {typeof(IGameControlsTransmitter)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(IGamePauser)}!");

            _mainMenuDisplay = mainMenuDisplay ?? throw new ArgumentNullException(nameof(mainMenuDisplay),
                $"Attempted to pass an empty {typeof(IMainMenuDisplay)}!");

            _levelSelectionDisplay = levelSelectionDisplay ?? throw new ArgumentNullException(nameof(levelSelectionDisplay),
                $"Attempted to pass an empty {typeof(ILevelSelectionDisplay)}!");

            _screenFader = screenFader ?? throw new ArgumentNullException(nameof(screenFader),
                $"Attempted to pass an empty {typeof(IScreenFader)}!");

            _levelDisplay = levelDisplay ?? throw new ArgumentNullException(nameof(levelDisplay),
                $"Attempted to pass an empty {typeof(ILevelDisplay)}!");

            _gamePauseDisplay = gamePauseDisplay ?? throw new ArgumentNullException(nameof(gamePauseDisplay),
                $"Attempted to pass an empty {typeof(IGamePauseDisplay)}!");
        }

        #region interfaces

        public void Initialize()
        {
            _mainMenuDisplay.EnableAsync().Forget();

            _gameControlsTransmitter.GoToPreviousMenu += GoToPreviousMenuEventHandler;

            _mainMenuDisplay.PlayButtonClicked += MainMenuPlayButtonClickedErventHandler;
            _mainMenuDisplay.InventoryButtonClicked += MainMenuInventroyButtonClickedEventHandler;
            _mainMenuDisplay.ArmoryButtonClicked += MainMenuArmoryButtonClickedEventHandler;
            _mainMenuDisplay.SettingsButtonClicked += MainMenuSettingsButtonClickedEventHandler;
            _mainMenuDisplay.StatisticsButtonClicked += MainMenuStatisticsButtonClickedEventHandler;
            _mainMenuDisplay.CreditsButtonClicked += MainMenuCreditsButtonClickedEventHandler;
            _mainMenuDisplay.CheatsButtonClicked += MainMenuCheatsButtonClickedEventHandler;
            _mainMenuDisplay.PointerOver += PointerOverEventHandler;

            _levelSelectionDisplay.MainMenuButtonClicked += LevelSelectionDisplayMainMenuButtonClickedEventHandler;
            _levelSelectionDisplay.BattleButtonClicked += async (sender, args) => await LevelSelectionDisplayBattleButtonClickedEventHandlerAsync(sender, args);
            _levelSelectionDisplay.LevelButtonChecked += LevelSelectionDisplayLevelButtonCheckedEventHandler;
            _levelSelectionDisplay.LevelButtonUnchecked += LevelSelectionDisplayLevelButtonUncheckedEventHandler;
            _levelSelectionDisplay.PointerOver += PointerOverEventHandler;

            _gamePauseDisplay.ResumeButtonClicked += GamePauseDisplayResumeButtonClickedEventHandler;
            _gamePauseDisplay.InventoryButtonClicked += GamePauseDisplayInventoryButtonClickedEventHandler;
            _gamePauseDisplay.SettingsButtonClicked += GamePauseDisplaySettingsButtonClickedEventHandler;
            _gamePauseDisplay.MainMenuButtonClicked += async (sender, args) => await GamePauseDisplayMainMenuButtonClickedEventHandlerAsync(sender, args);
            _gamePauseDisplay.PointerOver += PointerOverEventHandler;
        }

        public void Dispose()
        {
            _gameControlsTransmitter.GoToPreviousMenu -= GoToPreviousMenuEventHandler;

            _mainMenuDisplay.PlayButtonClicked -= MainMenuPlayButtonClickedErventHandler;
            _mainMenuDisplay.InventoryButtonClicked -= MainMenuInventroyButtonClickedEventHandler;
            _mainMenuDisplay.ArmoryButtonClicked -= MainMenuArmoryButtonClickedEventHandler;
            _mainMenuDisplay.SettingsButtonClicked -= MainMenuSettingsButtonClickedEventHandler;
            _mainMenuDisplay.StatisticsButtonClicked -= MainMenuStatisticsButtonClickedEventHandler;
            _mainMenuDisplay.CreditsButtonClicked -= MainMenuCreditsButtonClickedEventHandler;
            _mainMenuDisplay.CheatsButtonClicked -= MainMenuCheatsButtonClickedEventHandler;
            _mainMenuDisplay.PointerOver -= PointerOverEventHandler;

            _levelSelectionDisplay.MainMenuButtonClicked -= LevelSelectionDisplayMainMenuButtonClickedEventHandler;
            _levelSelectionDisplay.BattleButtonClicked -= async (sender, args) => await LevelSelectionDisplayBattleButtonClickedEventHandlerAsync(sender, args);
            _levelSelectionDisplay.LevelButtonChecked -= LevelSelectionDisplayLevelButtonCheckedEventHandler;
            _levelSelectionDisplay.LevelButtonUnchecked -= LevelSelectionDisplayLevelButtonUncheckedEventHandler;
            _levelSelectionDisplay.PointerOver -= PointerOverEventHandler;

            _gamePauseDisplay.ResumeButtonClicked -= GamePauseDisplayResumeButtonClickedEventHandler;
            _gamePauseDisplay.InventoryButtonClicked -= GamePauseDisplayInventoryButtonClickedEventHandler;
            _gamePauseDisplay.SettingsButtonClicked -= GamePauseDisplaySettingsButtonClickedEventHandler;
            _gamePauseDisplay.MainMenuButtonClicked -= async (sender, args) => await GamePauseDisplayMainMenuButtonClickedEventHandlerAsync(sender, args);
            _gamePauseDisplay.PointerOver -= PointerOverEventHandler;
        }

        #endregion

        #region event handlers

        private void GoToPreviousMenuEventHandler(object sender, CallbackContext e)
        {
            if (_mainMenuDisplay.Enabled == true) return;

            if (_levelSelectionDisplay.Enabled == true)
            {
                _levelSelectionDisplay.Disable();
                _mainMenuDisplay.EnableAsync().Forget();
            }

            if (_levelDisplay.Enabled == true)
            {
                _levelDisplay.Disable();
                _gamePauseDisplay.EnableAsync().Forget();
                _gamePauser.Pause();
            }

            if (_gamePauseDisplay.Enabled == true)
            {
                _gamePauseDisplay.Disable();
                _levelDisplay.EnableAsync().Forget();
                _gamePauser.Resume();
            }

            _audioPlayer.PlayOnceAsync(_uiAudio.BackwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        #region main menu

        private void MainMenuPlayButtonClickedErventHandler(object sender, EventArgs e)
        {
            _mainMenuDisplay.Disable();
            _levelSelectionDisplay.EnableAsync().Forget();
            _audioPlayer.PlayOnceAsync(_uiAudio.ForwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        private void MainMenuInventroyButtonClickedEventHandler(object sender, EventArgs e)
        {

        }

        private void MainMenuArmoryButtonClickedEventHandler(object sender, EventArgs e)
        {

        }

        private void MainMenuSettingsButtonClickedEventHandler(object sender, EventArgs e)
        {

        }

        private void MainMenuStatisticsButtonClickedEventHandler(object sender, EventArgs e)
        {

        }

        private void MainMenuCreditsButtonClickedEventHandler(object sender, EventArgs e)
        {

        }

        private void MainMenuCheatsButtonClickedEventHandler(object sender, EventArgs e)
        {
        }

        #endregion

        #region level selection menu

        private void LevelSelectionDisplayMainMenuButtonClickedEventHandler(object sender, EventArgs e)
        {
            _levelSelectionDisplay.Disable();
            _mainMenuDisplay.EnableAsync().Forget();
            _audioPlayer.PlayOnceAsync(_uiAudio.BackwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        private async UniTask LevelSelectionDisplayBattleButtonClickedEventHandlerAsync(object sender, EventArgs e)
        {
            _levelSelectionDisplay.Disable();
            _screenFader.FadeInAndOutAsync(FadingDuration).Forget();
            _audioPlayer.PlayOnceAsync(_uiAudio.ForwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();

            await _gameStateLoader.LoadLevelAsync(_selectedLevelIndex, _loadingDelay);

            _levelDisplay.EnableAsync().Forget();
        }

        private void LevelSelectionDisplayLevelButtonCheckedEventHandler(object sender, LevelButtonCheckedEventArgs e)
        {
            _selectedLevelIndex = e.LevelIndex;
            _audioPlayer.PlayOnceAsync(_uiAudio.ForwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        private void LevelSelectionDisplayLevelButtonUncheckedEventHandler(object sender, EventArgs e)
        {
            _selectedLevelIndex = 0;
        }

        #endregion

        #region game pause menu

        private void GamePauseDisplayResumeButtonClickedEventHandler(object sender, EventArgs e)
        {
            _gamePauseDisplay.Disable();
            _levelDisplay.EnableAsync().Forget();
            _gamePauser.Resume();
            _audioPlayer.PlayOnceAsync(_uiAudio.ForwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        private void GamePauseDisplayInventoryButtonClickedEventHandler(object sender, EventArgs e)
        {

        }

        private void GamePauseDisplaySettingsButtonClickedEventHandler(object sender, EventArgs e)
        {

        }

        private async UniTask GamePauseDisplayMainMenuButtonClickedEventHandlerAsync(object sender, EventArgs e)
        {
            _gamePauseDisplay.Disable();
            _screenFader.FadeInAndOutAsync(FadingDuration).Forget();
            _audioPlayer.PlayOnceAsync(_uiAudio.ForwardClick.Random, Vector3.zero, null, CancellationToken.None).Forget();

            await _gameStateLoader.LoadMainMenuAsync(_loadingDelay);

            _mainMenuDisplay.EnableAsync().Forget();
            _gamePauser.Resume();
        }

        #endregion

        private void PointerOverEventHandler(object sender, UnityEngine.UIElements.PointerOverEvent e)
        {
            _audioPlayer.PlayOnceAsync(_uiAudio.HoverOver.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        #endregion
    }
}