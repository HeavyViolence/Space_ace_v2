using SpaceAce.Gameplay.Players;

using UnityEngine;

using System;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterAudioListenerHolder : IMasterAudioListenerHolder, IInitializable, IDisposable
    {
        private readonly IPlayer _player;
        private readonly IGameStateLoader _gameStateLoader;

        public AudioListener MasterAudioListener { get; private set; }

        public MasterAudioListenerHolder(IMasterCameraHolder masterCameraHolder,
                                         IPlayer player,
                                         IGameStateLoader gameStateLoader)
        {
            if (masterCameraHolder is null)
                throw new ArgumentNullException(nameof(masterCameraHolder),
                    $"Attempted to pass an empty {typeof(IMasterCameraHolder)}!");

            AudioListener listener = masterCameraHolder.MasterCameraObject.GetComponentInChildren<AudioListener>();

            if (listener == null)
                throw new MissingComponentException($"Passed master camera object is missing {typeof(AudioListener)}!");

            MasterAudioListener = listener;

            _player = player ?? throw new ArgumentNullException(nameof(player),
                $"Attempted to pass an empty {typeof(IPlayer)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(IGameStateLoader)}!");
        }

        #region interfaces

        public void Initialize()
        {
            _player.SpaceshipSpawned += PlayerSpaceshipSpawnedEventHandler;
            _player.SpaceshipDefeated += PlayerSpaceshipDefeatedEventHandler;

            _gameStateLoader.MainMenuLoadingStarted += MainMenuLoadingStartedEventHandler;
        }

        public void Dispose()
        {
            _player.SpaceshipSpawned -= PlayerSpaceshipSpawnedEventHandler;
            _player.SpaceshipDefeated -= PlayerSpaceshipDefeatedEventHandler;

            _gameStateLoader.MainMenuLoadingStarted -= MainMenuLoadingStartedEventHandler;
        }

        #endregion

        #region event handlers

        private void PlayerSpaceshipSpawnedEventHandler(object sender, EventArgs e)
        {
            MasterAudioListener.enabled = false;
        }

        private void PlayerSpaceshipDefeatedEventHandler(object sender, EventArgs e)
        {
            MasterAudioListener.enabled = true;
        }

        private void MainMenuLoadingStartedEventHandler(object sender, MainMenuLoadingStartedEventArgs e)
        {
            MasterAudioListener.enabled = true;
        }

        #endregion
    }
}