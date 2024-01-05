using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Players;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelCompleter : IInitializable, IDisposable
    {
        public event EventHandler<LevelEndedEventArgs> LevelCompleted;
        public event EventHandler<LevelEndedEventArgs> LevelFailed;
        public event EventHandler<LevelEndedEventArgs> LevelConcluded;

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
            if (levelCompletedAudio == null)
                throw new ArgumentNullException(nameof(levelCompletedAudio),
                    $"Attempted to pass an empty {typeof(AudioCollection)}!");

            _levelCompletedAudio = levelCompletedAudio;

            if (levelFailedAudio == null) throw new ArgumentNullException(nameof(levelFailedAudio),
                    $"Attempted to pass an empty {typeof(AudioCollection)}!");

            _levelFailedAudio = levelFailedAudio;

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _player = player ?? throw new ArgumentNullException(nameof(player),
                $"Attempted to pass an empty {typeof(Players.Player)}!");

            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer),
                $"Attempted to pass an empty {typeof(AudioPlayer)}!");
        }

        #region interfaces

        public void Initialize()
        {
            _gameStateLoader.MainMenuLoadingStarted += MainMenuLoadingStartedEventHandler;
            _player.SpaceshipDefeated += PlayerSpaceshipDefeatedEventHandler;
        }

        public void Dispose()
        {
            _gameStateLoader.MainMenuLoadingStarted -= MainMenuLoadingStartedEventHandler;
            _player.SpaceshipDefeated -= PlayerSpaceshipDefeatedEventHandler;
        }

        #endregion

        #region event handlers

        private void MainMenuLoadingStartedEventHandler(object sender, MainMenuLoadingStartedEventArgs e)
        {
            LevelConcluded?.Invoke(this, new(_gameStateLoader.LoadedLevelIndex));
        }

        private void PlayerSpaceshipDefeatedEventHandler(object sender, EventArgs e)
        {
            LevelConcluded?.Invoke(this, new(_gameStateLoader.LoadedLevelIndex));
            LevelFailed?.Invoke(this, new(_gameStateLoader.LoadedLevelIndex));
            _audioPlayer.PlayOnceAsync(_levelFailedAudio.Random, Vector3.zero, null, CancellationToken.None).Forget();
        }

        #endregion
    }
}