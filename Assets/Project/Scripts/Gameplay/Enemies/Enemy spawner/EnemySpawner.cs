using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Main;
using SpaceAce.Main.Factories.AmmoFactories;
using SpaceAce.Main.Factories.EnemyShipFactories;

using System;
using System.Collections.Generic;
using System.Threading;

using Zenject;

namespace SpaceAce.Gameplay.Enemies
{
    public sealed class EnemySpawner : IInitializable, IDisposable
    {
        public event EventHandler SpawnStarted, SpawnEnded;
        public event EventHandler<EnemySpawnedEventArgs> EnemySpawned;

        private readonly EnemySpawnerConfig _config;
        private readonly GameStateLoader _gameStateLoader;
        private readonly AmmoFactory _ammoFactory;
        private readonly EnemyShipFactory _enemyShipFactory;
        private readonly MasterCameraHolder _masterCameraHolder;
        private readonly GamePauser _gamePauser;
        private readonly HashSet<Enemy> _aliveEnemies = new();

        private CancellationTokenSource _spawnCancellation;

        public int AmountLeftToSpawn => _config.AmountLeftToSpawn;
        public int AmountToSpawnThisLevel => _config.AmountToSpawnThisLevel;

        public EnemySpawner(EnemySpawnerConfig config,
                            GameStateLoader gameStateLoader,
                            AmmoFactory ammoFactory,
                            EnemyShipFactory enemyShipFactory,
                            MasterCameraHolder masterCameraHolder,
                            GamePauser gamePauser)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _ammoFactory = ammoFactory ?? throw new ArgumentNullException();
            _enemyShipFactory = enemyShipFactory ?? throw new ArgumentNullException();
            _masterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
        }

        private async UniTask SpawnAsync(int level, CancellationToken token = default)
        {
            SpawnStarted?.Invoke(this, EventArgs.Empty);

            while (_config.TryGetNextWave(level, out IEnumerable<(EnemyConfig config, float spawnDelay)> wave) == true)
            {
                foreach ((EnemyConfig config, float spawnDelay) in wave)
                {
                    await AuxAsync.DelayAsync(() => spawnDelay, () => _gamePauser.Paused == true, token);

                    if (token.IsCancellationRequested == true)
                    {
                        SpawnEnded?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    Enemy enemy = new(config, _ammoFactory, _enemyShipFactory, _masterCameraHolder, _gamePauser);
                    _aliveEnemies.Add(enemy);
                    EnemySpawned?.Invoke(this, new(enemy, false));

                    enemy.Defeated += EnemyDefeatedEventHandler;
                }

                await UniTask.WaitUntil(() => _aliveEnemies.Count == 0, PlayerLoopTiming.Update, token);
            }

            if (token.IsCancellationRequested == false &&
                _config.TryGetNextBoss(level, out EnemyConfig bossConfig) == true)
            {
                await UniTask.WaitForSeconds(_config.BossSpawnDelay);

                Enemy boss = new(bossConfig, _ammoFactory, _enemyShipFactory, _masterCameraHolder, _gamePauser);
                _aliveEnemies.Add(boss);
                EnemySpawned?.Invoke(this, new(boss, true));

                boss.Defeated += EnemyDefeatedEventHandler;
            }

            SpawnEnded?.Invoke(this, EventArgs.Empty);
        }

        #region interfaces

        public void Initialize()
        {
            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
        }

        public void Dispose()
        {
            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
        }

        #endregion

        #region event handlers

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            _config.Configure(e.Level);

            _spawnCancellation = new();
            SpawnAsync(e.Level, _spawnCancellation.Token).Forget();
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            _spawnCancellation.Cancel();
            _spawnCancellation.Dispose();
            _spawnCancellation = null;

            foreach (Enemy enemy in _aliveEnemies)
                enemy.Dispose();

            _aliveEnemies.Clear();
        }

        private void EnemyDefeatedEventHandler(object sender, DestroyedEventArgs e)
        {
            Enemy enemy = sender as Enemy;
            enemy.Dispose();

            _aliveEnemies.Remove(enemy);
        }

        #endregion
    }
}