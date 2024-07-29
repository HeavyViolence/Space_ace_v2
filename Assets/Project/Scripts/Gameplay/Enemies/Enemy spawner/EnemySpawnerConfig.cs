using NaughtyAttributes;

using SpaceAce.Auxiliary;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "Enemy spawner config",
                     menuName = "Space ace/Configs/Enemies/Enemy spawner config")]
    public sealed class EnemySpawnerConfig : ScriptableObject, IInitializable
    {
        #region enemies

        [SerializeField]
        private List<EnemyConfig> _enemies;

        [SerializeField]
        private AnimationCurve _spawnProbability;

        private Dictionary<Vector2, EnemyConfig> _enemyDistribution;

        public bool TryGetNextWave(int level, out IEnumerable<(EnemyConfig config, float spawnDelay)> wave)
        {
            if (level < 1) throw new ArgumentOutOfRangeException();

            int waveLength = Mathf.Clamp(NextWaveLength, 0, AmountToSpawn);

            if (waveLength == 0)
            {
                wave = Enumerable.Empty<(EnemyConfig config, float spawnDelay)>();
                return false;
            }

            List<(EnemyConfig config, float spawnDelay)> result = new(waveLength);

            for (int i = 0; i < waveLength; i++)
            {
                EnemyConfig config = GetProbableEnemyConfig(level);
                float spawnDelay = GetSpawnDelay(i == 0);

                result.Add((config, spawnDelay));
                AmountToSpawn--;
            }

            wave = result;
            return true;
        }

        private EnemyConfig GetProbableEnemyConfig(int level)
        {
            if (_enemyDistribution is null)
                throw new Exception($"Enemies are not assigned in the {nameof(EnemySpawnerConfig)}!");

            float seed = AuxMath.RandomNormal;

            foreach (var entry in _enemyDistribution)
            {
                if (AuxMath.ValueInRange(entry.Key, seed) == true &&
                    entry.Value.FirstLevelToEncounter <= level)
                {
                    return entry.Value;
                }
            }


            return _enemyDistribution.Last().Value;
        }

        #endregion

        #region bosses

        public const int MinBossPerLevelsPassed = 1;
        public const int MaxBossPerLevelsPassed = 10;

        [SerializeField, Space]
        private List<EnemyConfig> _bosses;

        [SerializeField, Range(MinBossPerLevelsPassed, MaxBossPerLevelsPassed)]
        private int _bossPerLevelsPassed = MinBossPerLevelsPassed;

        public bool TryGetNextBoss(int level, out EnemyConfig config)
        {
            if (level % _bossPerLevelsPassed == 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, _bosses.Count);

                config = _bosses[randomIndex];
                return true;
            }

            config = null;
            return false;
        }

        #endregion

        #region boss spawn delay

        public const float MinBossSpawnDelay = 0f;
        public const float MaxBossSpawnDelay = 10f;

        [SerializeField, MinMaxSlider(MinBossSpawnDelay, MaxBossSpawnDelay)]
        private Vector2 _bossSpawnDelay = new(MinBossSpawnDelay, MaxBossSpawnDelay);

        public float BossSpawnDelay => UnityEngine.Random.Range(_bossSpawnDelay.x, _bossSpawnDelay.y);

        #endregion

        #region amount to spawn

        public const int MinAmountToSpawn = 1;
        public const int MaxAmountToSpawn = 100;

        [SerializeField, MinMaxSlider(MinAmountToSpawn, MaxAmountToSpawn), Space]
        private Vector2Int _amountToSpawn = new(MinAmountToSpawn, MaxAmountToSpawn);

        public int AmountToSpawn { get; private set; } = 0;

        #endregion

        #region wave length

        public const int MinWaveLength = 1;
        public const int MaxWaveLength = 10;

        [SerializeField, MinMaxSlider(MinWaveLength, MaxWaveLength)]
        private Vector2Int _waveLength = new(MinWaveLength, MaxWaveLength);

        private int NextWaveLength => UnityEngine.Random.Range(_waveLength.x, _waveLength.y + 1);

        #endregion

        #region spawn delay

        public const float MinWaveDelay = 0f;
        public const float MaxWaveDelay = 10f;

        public const float MinSpawnDelay = 0f;
        public const float MaxSpawnDelay = 10f;

        [SerializeField, MinMaxSlider(MinWaveDelay, MaxWaveDelay), Space]
        private Vector2 _waveDelay = new(MinWaveDelay, MaxWaveDelay);

        [SerializeField, MinMaxSlider(MinSpawnDelay, MaxSpawnDelay)]
        private Vector2 _spawnDelay = new(MinSpawnDelay, MaxSpawnDelay);

        private float GetSpawnDelay(bool firstInWave) =>
            firstInWave == true ? UnityEngine.Random.Range(_waveDelay.x, _waveDelay.y)
                                : UnityEngine.Random.Range(_spawnDelay.x, _spawnDelay.y);

        #endregion

        private void OnEnable()
        {
            if (_enemies is null || _enemies.Count == 0) return;
            _enemyDistribution = AuxMath.InterpolateValuesByRange(_spawnProbability, _enemies);
        }

        public void Initialize()
        {
            AmountToSpawn = UnityEngine.Random.Range(_amountToSpawn.x, _amountToSpawn.y + 1);
        }
    }
}