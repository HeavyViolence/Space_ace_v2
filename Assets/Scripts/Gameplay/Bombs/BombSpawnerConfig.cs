using NaughtyAttributes;

using SpaceAce.Auxiliary;
using SpaceAce.Main.Factories.BombFactories;

using System;
using System.Linq;

using UnityEngine;

namespace SpaceAce.Gameplay.Bombs
{
    [CreateAssetMenu(fileName = "Bomb spawner config",
                     menuName = "Space ace/Configs/Hazards/Bomb spawner config")]
    public sealed class BombSpawnerConfig : ScriptableObject
    {
        #region speed

        public const float MinSpeed = 0f;
        public const float MaxSpeed = 50f;

        [SerializeField, HorizontalLine, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _speed = new(MinSpeed, MaxSpeed);

        [SerializeField]
        private AnimationCurve _speedProbability;

        private float GetSpeed()
        {
            float speedNormal = _speedProbability.Evaluate(AuxMath.RandomNormal);
            float speed = Mathf.Lerp(_speed.x, _speed.y, speedNormal);

            return speed;
        }

        #endregion

        #region wave delay

        public const float MinWaveDelay = 1f;
        public const float MaxWaveDelay = 300f;

        [SerializeField, HorizontalLine, MinMaxSlider(MinWaveDelay, MaxWaveDelay)]
        private Vector2 _waveDelay = new(MinWaveDelay, MaxWaveDelay);

        private float GetWaveDelay() => UnityEngine.Random.Range(_waveDelay.x, _waveDelay.y);

        #endregion

        #region spawn delay

        public const float MinSpawnDelay = 1f;
        public const float MaxSpawnDelay = 60f;

        [SerializeField, MinMaxSlider(MinSpawnDelay, MaxSpawnDelay)]
        private Vector2 _spawnDelay = new(MinSpawnDelay, MaxSpawnDelay);

        private float GetSpawnDelay() => UnityEngine.Random.Range(_spawnDelay.x, _spawnDelay.y);

        #endregion

        #region wave length

        public const int MinWaveLength = 1;
        public const int MaxWaveLength = 100;

        [SerializeField, MinMaxSlider(MinWaveLength, MaxWaveLength)]
        private Vector2Int _waveLength = new(MinWaveLength, MaxWaveLength);

        private int GetWaveLength() => UnityEngine.Random.Range(_waveLength.x, _waveLength.y);

        #endregion

        #region spawn width

        [SerializeField]
        private AnimationCurve _spawnWidthProbability;

        public float GetSpawnWidth(float minWidth, float maxWidth)
        {
            float widthNormalized = _spawnWidthProbability.Evaluate(AuxMath.RandomNormal);
            float width = Mathf.Lerp(minWidth, maxWidth, widthNormalized);

            return width;
        }

        #endregion

        #region API

        public BombWave NextWave(int levelIndex)
        {
            if (levelIndex <= 0) throw new ArgumentOutOfRangeException();

            int waveLength = GetWaveLength();
            BombWaveSlot[] waveSlots = new BombWaveSlot[waveLength];
            BombSize[] bombSequenceThisWave = GetBombSizeRandomSequence(waveLength);

            for (int i = 0; i < waveLength; i++)
            {
                BombSize size = bombSequenceThisWave[i];
                float spawnDelay = i == 0 ? GetWaveDelay() : GetSpawnDelay();
                float speed = GetSpeed();

                BombWaveSlot slot = new(size, spawnDelay, speed);
                waveSlots[i] = slot;
            }

            return new(waveSlots);
        }

        private BombSize[] GetBombSizeRandomSequence(int length)
        {
            BombSize[] sequence = new BombSize[length];
            BombSize[] bombSizesTotal = Enum.GetValues(typeof(BombSize)).Cast<BombSize>().ToArray();

            for (int i = 0; i < length; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, bombSizesTotal.Length);
                sequence[i] = bombSizesTotal[randomIndex];
            }

            return sequence;
        }

        #endregion
    }
}