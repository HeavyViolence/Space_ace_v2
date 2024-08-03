using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Bombs;
using SpaceAce.Gameplay.Enemies;
using SpaceAce.Gameplay.Meteors;
using SpaceAce.Gameplay.Wrecks;
using SpaceAce.Main;

using System;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelRewardCollector : IInitializable, IDisposable
    {
        public event EventHandler<FloatValueChangedEventArgs> CreditsRewardChanged, ExperienceRewardChanged;

        private readonly LevelRewardCollectorConfig _config;
        private readonly GameStateLoader _gameStateLoader;
        private readonly MeteorSpawner _meteorSpawnner;
        private readonly WreckSpawner _wreckSpawner;
        private readonly BombSpawner _bombSpawner;
        private readonly EnemySpawner _enemySpawner;

        private float _creditsReward = 0f;
        public float CreditsReward
        {
            get => _creditsReward;

            private set
            {
                CreditsRewardChanged?.Invoke(this, new(_creditsReward, value));
                _creditsReward = value;
            }
        }

        private float _experienceReward = 0f;
        public float ExperienceReward
        {
            get => _experienceReward;

            private set
            {
                ExperienceRewardChanged?.Invoke(this, new(_experienceReward, value));
                _experienceReward = value;
            }
        }

        public LevelRewardCollector(LevelRewardCollectorConfig config,
                                    GameStateLoader gameStateLoader,
                                    MeteorSpawner meteorSpawner,
                                    WreckSpawner wreckSpawner,
                                    BombSpawner bombSpawner,
                                    EnemySpawner enemySpwner)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _meteorSpawnner = meteorSpawner ?? throw new ArgumentNullException();
            _wreckSpawner = wreckSpawner ?? throw new ArgumentNullException();
            _bombSpawner = bombSpawner ?? throw new ArgumentNullException();
            _enemySpawner = enemySpwner ?? throw new ArgumentNullException();
        }

        #region interfaces

        public void Initialize()
        {
            _gameStateLoader.LevelLoadingStarted += LevelLoadingStartedEventHandler;
            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;

            _meteorSpawnner.MeteorSpawned += MeteorSpawnerEventHandler;
            _wreckSpawner.WreckSpawned += WreckSpawnedEventHandler;
            _bombSpawner.BombSpawned += BombSpawnedEventHandler;
            _enemySpawner.EnemySpawned += EnemySpawnedEventHandler;
        }

        public void Dispose()
        {
            _gameStateLoader.LevelLoadingStarted -= LevelLoadingStartedEventHandler;
            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;

            _meteorSpawnner.MeteorSpawned -= MeteorSpawnerEventHandler;
            _wreckSpawner.WreckSpawned -= WreckSpawnedEventHandler;
            _bombSpawner.BombSpawned -= BombSpawnedEventHandler;
            _enemySpawner.EnemySpawned -= EnemySpawnedEventHandler;
        }

        #endregion

        #region event handlers

        private void LevelLoadingStartedEventHandler(object sender, LevelLoadingStartedEventArgs e)
        {
            CreditsReward = _config.GetBaseCreditsReward(e.Level);
            ExperienceReward = _config.GetBaseExperienceReward(e.Level);
        }

        private void MainMenuLoadedEventHandler(object sender, EventArgs e)
        {
            CreditsReward = 0f;
            ExperienceReward = 0f;
        }

        private void MeteorSpawnerEventHandler(object sender, MeteorSpawnedEventArgs e)
        {
            e.View.Destroyable.Destroyed += (_, e) => ExperienceReward += e.Experience.Gain;
        }

        private void WreckSpawnedEventHandler(object sender, WreckSpawnedEventArgs e)
        {
            e.View.Destroyable.Destroyed += (_, e) => ExperienceReward += e.Experience.Gain;
        }

        private void BombSpawnedEventHandler(object sender, BombSpawnedEventArgs e)
        {
            e.View.Destroyable.Destroyed += (_, e) => ExperienceReward += e.Experience.Gain;
        }

        private void EnemySpawnedEventHandler(object sender, EnemySpawnedEventArgs e)
        {
            e.Enemy.View.Destroyable.Destroyed += (_, e) => ExperienceReward += e.Experience.Gain;
        }

        private void BossSpawnedEventHandler(object sender, EnemySpawnedEventArgs e)
        {
            e.Enemy.View.Destroyable.Destroyed += (_, e) => ExperienceReward += e.Experience.Gain;
        }

        #endregion
    }
}