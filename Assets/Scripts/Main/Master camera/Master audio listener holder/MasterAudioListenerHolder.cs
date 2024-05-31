using SpaceAce.Gameplay.Players;

using UnityEngine;

using System;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterAudioListenerHolder : IInitializable, IDisposable
    {
        private readonly Player _player;
        private readonly GameStateLoader _gameStateLoader;

        public AudioListener MasterAudioListener { get; private set; }

        public MasterAudioListenerHolder(MasterCameraHolder masterCameraHolder,
                                         Player player,
                                         GameStateLoader gameStateLoader)
        {
            if (masterCameraHolder is null) throw new ArgumentNullException();

            AudioListener listener = masterCameraHolder.MasterCameraAnchor.gameObject.GetComponentInChildren<AudioListener>();

            if (listener == null) throw new MissingComponentException(nameof(AudioListener));

            MasterAudioListener = listener;

            _player = player ?? throw new ArgumentNullException();
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
        }

        #region interfaces

        public void Initialize()
        {
            _player.ShipSpawned += PlayerSpaceshipSpawnedEventHandler;
            _player.ShipDefeated += PlayerSpaceshipDefeatedEventHandler;

            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
        }

        public void Dispose()
        {
            _player.ShipSpawned -= PlayerSpaceshipSpawnedEventHandler;
            _player.ShipDefeated -= PlayerSpaceshipDefeatedEventHandler;

            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
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

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            MasterAudioListener.enabled = true;
        }

        #endregion
    }
}