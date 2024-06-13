using NaughtyAttributes;

using SpaceAce.Auxiliary;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories.WreckFactories;

using System;
using System.Linq;

using UnityEngine;

namespace SpaceAce.Gameplay.Wrecks
{
    [CreateAssetMenu(fileName = "Wreck spawner config",
                     menuName = "Space ace/Configs/Hazards/Wreck spawner config")]
    public sealed class WreckSpawnerConfig : ScriptableObject
    {
        #region wreck showers

        public const int MinWreckShowerLevelIndex = 1;
        public const int MaxWreckShowerLevelIndex = 100;

        [SerializeField, HorizontalLine, MinMaxSlider(MinWreckShowerLevelIndex, MaxWreckShowerLevelIndex)]
        private Vector2Int _wreckShowerLevels = new(MinWreckShowerLevelIndex, MaxWreckShowerLevelIndex);

        [SerializeField]
        private AnimationCurve _wreckShowerProbability;

        private bool IsWreckShower(int level)
        {
            float evaluator = Mathf.InverseLerp(_wreckShowerLevels.x, _wreckShowerLevels.y, level);
            float wreckShowerProbability = _wreckShowerProbability.Evaluate(evaluator);

            return AuxMath.RandomNormal < wreckShowerProbability;
        }

        #endregion

        #region scale

        public const float MinScale = 0.1f;
        public const float MaxScale = 10f;

        [SerializeField, HorizontalLine, MinMaxSlider(MinScale, MaxScale)]
        private Vector2 _scale = new(MinScale, MaxScale);

        [SerializeField]
        private AnimationCurve _scaleProbability;

        private Vector3 GetScale()
        {
            float scaleNormal = _scaleProbability.Evaluate(AuxMath.RandomNormal);
            float scale = Mathf.Lerp(_scale.x, _scale.y, scaleNormal);

            return new(scale, scale, scale);
        }

        #endregion

        #region speed

        public const float MinSpeed = 0f;
        public const float MaxSpeed = 50f;

        [SerializeField, HorizontalLine, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _speed = new(MinSpeed, MaxSpeed);

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _showerSpeed = new(MinSpeed, MaxSpeed);

        [SerializeField]
        private AnimationCurve _speedProbability;

        private float GetSpeed(bool meteorShower)
        {
            float speedNormal = _speedProbability.Evaluate(AuxMath.RandomNormal);
            float speed;

            if (meteorShower == true) speed = Mathf.Lerp(_showerSpeed.x, _showerSpeed.y, speedNormal);
            else speed = Mathf.Lerp(_speed.x, _speed.y, speedNormal);

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

        [SerializeField, HorizontalLine, MinMaxSlider(MinSpawnDelay, MaxSpawnDelay)]
        private Vector2 _spawnDelay = new(MinSpawnDelay, MaxSpawnDelay);

        [SerializeField, MinMaxSlider(MinSpawnDelay, MaxSpawnDelay)]
        private Vector2 _showerSpawnDelay = new(MinSpawnDelay, MaxSpawnDelay);

        private float GetSpawnDelay(bool meteorShower)
        {
            float delay;

            if (meteorShower == true) delay = UnityEngine.Random.Range(_showerSpawnDelay.x, _showerSpawnDelay.y);
            else delay = UnityEngine.Random.Range(_spawnDelay.x, _spawnDelay.y);

            return delay;
        }

        #endregion

        #region wave length

        public const int MinWaveLength = 1;
        public const int MaxWaveLength = 100;

        [SerializeField, HorizontalLine, MinMaxSlider(MinWaveLength, MaxWaveLength)]
        private Vector2Int _waveLength = new(MinWaveLength, MaxWaveLength);

        [SerializeField, MinMaxSlider(MinWaveLength, MaxWaveLength)]
        private Vector2Int _showerWaveLength = new(MinWaveLength, MaxWaveLength);

        private int GetWaveLength(bool meteorShower)
        {
            int length;

            if (meteorShower == true) length = UnityEngine.Random.Range(_showerWaveLength.x, _showerWaveLength.y);
            else length = UnityEngine.Random.Range(_waveLength.x, _waveLength.y);

            return length;
        }

        #endregion

        #region spawn width

        [SerializeField, HorizontalLine]
        private AnimationCurve _spawnWidthProbability;

        public float GetSpawnWidth(float minWidth, float maxWidth)
        {
            float widthNormalized = _spawnWidthProbability.Evaluate(AuxMath.RandomNormal);
            float width = Mathf.Lerp(minWidth, maxWidth, widthNormalized);

            return width;
        }

        #endregion

        #region target width

        [SerializeField]
        private AnimationCurve _targetWidthProbability;

        public float GetTargetWidth(float minWidth, float maxWidth)
        {
            float widthNormalized = _spawnWidthProbability.Evaluate(AuxMath.RandomNormal);
            float width = Mathf.Lerp(minWidth, maxWidth, widthNormalized);

            return width;
        }

        #endregion

        #region API

        public WreckWave NextWave(int level)
        {
            if (level <= 0) throw new ArgumentOutOfRangeException();

            bool isWreckShower = IsWreckShower(level);
            int waveLength = GetWaveLength(isWreckShower);
            WreckWaveSlot[] waveSlots = new WreckWaveSlot[waveLength];
            WreckType[] wreckSequenceThisWave = GetWreckTypeRandomSequence(waveLength);

            for (int i = 0; i < waveLength; i++)
            {
                WreckType type = wreckSequenceThisWave[i];
                Vector3 scale = GetScale();
                float spawnDelay = i == 0 ? GetWaveDelay() : GetSpawnDelay(isWreckShower);
                float speed = GetSpeed(isWreckShower);

                WreckWaveSlot slot = new(type, scale, spawnDelay, speed);
                waveSlots[i] = slot;
            }

            return new(waveSlots, isWreckShower);
        }

        private WreckType[] GetWreckTypeRandomSequence(int length)
        {
            WreckType[] sequence = new WreckType[length];
            WreckType[] wreckTypesTotal = Enum.GetValues(typeof(WreckType)).Cast<WreckType>().ToArray();

            for (int i = 0; i < length; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, wreckTypesTotal.Length);
                sequence[i] = wreckTypesTotal[randomIndex];
            }

            return sequence;
        }

        #endregion

        #region collisions

        public const float MinCollisionDamage = 0f;
        public const float MaxCollisionDamage = 10_000f;

        [SerializeField, HorizontalLine, MinMaxSlider(MinCollisionDamage, MaxCollisionDamage)]
        private Vector2 _collisionDamage = new(MinCollisionDamage, MaxCollisionDamage);

        public float GetCollisionDamage() => UnityEngine.Random.Range(_collisionDamage.x, _collisionDamage.y);

        [SerializeField]
        private AudioCollection _collisionAudio;

        public AudioCollection CollisionAudio => _collisionAudio;

        #endregion
    }
}