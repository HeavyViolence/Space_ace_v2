using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Players;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelCompleter : IInitializable, IDisposable
    {
        public event EventHandler<LevelEndedEventArgs> LevelCompleted, LevelFailed, LevelConcluded;

        private readonly AudioCollection _levelCompletedAudio;
        private readonly AudioCollection _levelFailedAudio;
        private readonly GameStateLoader _gameStateLoader;
        private readonly Player _player;
        private readonly AudioPlayer _audioPlayer;

        public LevelCompleter(AudioCollection levelCompletedAudio,
                              AudioCollection levelFailedAudio,
                              GameStateLoader gameStateLoader,
                              Player player,
                              AudioPlayer audioPlayer)
        {
            if (levelCompletedAudio == null) throw new ArgumentNullException();
            _levelCompletedAudio = levelCompletedAudio;

            if (levelFailedAudio == null) throw new ArgumentNullException();
            _levelFailedAudio = levelFailedAudio;

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _player = player ?? throw new ArgumentNullException();
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException();
        }

        #region interfaces

        public void Initialize()
        {
            _gameStateLoader.MainMenuLoadingStarted += MainMenuLoadingStartedEventHandler;
            _player.ShipDefeated += PlayerShipDefeatedEventHandler;
        }

        public void Dispose()
        {
            _gameStateLoader.MainMenuLoadingStarted -= MainMenuLoadingStartedEventHandler;
            _player.ShipDefeated -= PlayerShipDefeatedEventHandler;
        }

        #endregion

        #region event handlers

        private void MainMenuLoadingStartedEventHandler(object sender, MainMenuLoadingStartedEventArgs e)
        {
            LevelConcluded?.Invoke(this, new(_gameStateLoader.LoadedLevel));
        }

        private void PlayerShipDefeatedEventHandler(object sender, EventArgs e)
        {
            LevelConcluded?.Invoke(this, new(_gameStateLoader.LoadedLevel));
            LevelFailed?.Invoke(this, new(_gameStateLoader.LoadedLevel));
            _audioPlayer.PlayOnceAsync(_levelFailedAudio.Random, Vector3.zero).Forget();
        }

        #endregion
    }
}