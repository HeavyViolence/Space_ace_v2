using SpaceAce.Architecture;
using SpaceAce.Gameplay.Players;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelsCompleter : IInitializable, IDisposable
    {
        public event EventHandler<LevelEndedEventArgs> LevelCompleted;
        public event EventHandler<LevelEndedEventArgs> LevelFailed;
        public event EventHandler<LevelEndedEventArgs> LevelConcluded;

        private readonly GameStateLoader _gameStateLoader = null;
        private readonly Player _player = null;
        private readonly AudioCollection _levelCompletedAudio = null;
        private readonly AudioCollection _levelFailedAudio = null;

        public LevelsCompleter(GameStateLoader gameStateLoader,
                               Player player,
                               AudioCollection levelCompletedAudio,
                               AudioCollection levelFailedAudio)
        {
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _player = player ?? throw new ArgumentNullException(nameof(player),
                $"Attempted to pass an empty {typeof(Player)}!");

            if (levelCompletedAudio == null)
                throw new ArgumentNullException(nameof(levelCompletedAudio),
                    $"Attempted to pass an empty level completion audio: {typeof(AudioCollection)}!");

            if (levelFailedAudio == null)
                throw new ArgumentNullException(nameof(levelFailedAudio),
                    $"Attempted to pass an empty level failed audio: {typeof(AudioCollection)}!");

            _levelCompletedAudio = levelCompletedAudio;
            _levelFailedAudio = levelFailedAudio;
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

            _levelFailedAudio.PlayRandomAudioClip(Vector3.zero);
        }

        #endregion
    }
}