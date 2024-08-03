using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Enemies;
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
        private readonly EnemySpawner _enemySpawner;

        private int _enemiesDefeated;

        public LevelCompleter(AudioCollection levelCompletedAudio,
                              AudioCollection levelFailedAudio,
                              GameStateLoader gameStateLoader,
                              Player player,
                              AudioPlayer audioPlayer,
                              EnemySpawner enemySpawner)
        {
            if (levelCompletedAudio == null) throw new ArgumentNullException();
            _levelCompletedAudio = levelCompletedAudio;

            if (levelFailedAudio == null) throw new ArgumentNullException();
            _levelFailedAudio = levelFailedAudio;

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _player = player ?? throw new ArgumentNullException();
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException();
            _enemySpawner = enemySpawner ?? throw new ArgumentNullException();
        }

        #region interfaces

        public void Initialize()
        {
            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoadingStarted += MainMenuLoadingStartedEventHandler;

            _player.ShipDefeated += PlayerShipDefeatedEventHandler;
        }

        public void Dispose()
        {
            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoadingStarted -= MainMenuLoadingStartedEventHandler;

            _player.ShipDefeated -= PlayerShipDefeatedEventHandler;
        }

        #endregion

        #region event handlers

        private void MainMenuLoadingStartedEventHandler(object sender, MainMenuLoadingStartedEventArgs e)
        {
            LevelConcluded?.Invoke(this, new(_gameStateLoader.LoadedLevel));
            _enemiesDefeated = 0;
        }

        private void PlayerShipDefeatedEventHandler(object sender, EventArgs e)
        {
            LevelConcluded?.Invoke(this, new(_gameStateLoader.LoadedLevel));
            LevelFailed?.Invoke(this, new(_gameStateLoader.LoadedLevel));
            _audioPlayer.PlayOnceAsync(_levelFailedAudio.Random, Vector3.zero).Forget();
        }

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs args)
        {
            _enemySpawner.EnemySpawned += (_, e) =>
            {
                e.Enemy.View.Destroyable.Destroyed += (_, _) =>
                {
                    if (++_enemiesDefeated == _enemySpawner.AmountToSpawnThisLevel)
                        LevelCompleted?.Invoke(this, new(args.Level));
                };
            };
        }

        #endregion
    }
}