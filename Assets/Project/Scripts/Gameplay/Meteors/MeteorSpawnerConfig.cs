using NaughtyAttributes;

using SpaceAce.Auxiliary;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories.MeteorFactories;

using System;
using System.Linq;

using UnityEngine;

namespace SpaceAce.Gameplay.Meteors
{
    [CreateAssetMenu(fileName = "Meteor spawner config",
                     menuName = "Space ace/Configs/Hazards/Meteor spawner config")]
    public sealed class MeteorSpawnerConfig : ScriptableObject
    {
        #region meteor showers

        public const int MinMeteorShowerLevelIndex = 1;
        public const int MaxMeteorShowerLevelIndex = 100;

        [SerializeField, HorizontalLine, MinMaxSlider(MinMeteorShowerLevelIndex, MaxMeteorShowerLevelIndex)]
        private Vector2Int _meteorShowerLevels = new(MinMeteorShowerLevelIndex, MaxMeteorShowerLevelIndex);

        [SerializeField]
        private AnimationCurve _meteorShowerProbability;

        private bool IsMeteorShower(int level)
        {
            float evaluator = Mathf.InverseLerp(_meteorShowerLevels.x, _meteorShowerLevels.y, level);
            float meteorShowerProbability = _meteorShowerProbability.Evaluate(evaluator);

            return AuxMath.RandomNormal < meteorShowerProbability;
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

        #region API

        public MeteorWave NextWave(int level)
        {
            if (level <= 0) throw new ArgumentOutOfRangeException();

            bool isMeteorShower = IsMeteorShower(level);
            int waveLength = GetWaveLength(isMeteorShower);
            MeteorWaveSlot[] waveSlots = new MeteorWaveSlot[waveLength];
            MeteorType[] meteorSequenceThisWave = GetMeteorTypeRandomSequence(waveLength);

            for (int i = 0; i < waveLength; i++)
            {
                MeteorType type = meteorSequenceThisWave[i];
                Vector3 scale = GetScale();
                float spawnDelay = i == 0 ? GetWaveDelay() : GetSpawnDelay(isMeteorShower);
                float speed = GetSpeed(isMeteorShower);

                MeteorWaveSlot slot = new(type, scale, spawnDelay, speed);
                waveSlots[i] = slot;
            }

            return new(waveSlots, isMeteorShower);
        }

        private MeteorType[] GetMeteorTypeRandomSequence(int length)
        {
            MeteorType[] sequence = new MeteorType[length];
            MeteorType[] meteorTypesTotal = Enum.GetValues(typeof(MeteorType)).Cast<MeteorType>().ToArray();

            for (int i = 0; i < length; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, meteorTypesTotal.Length);
                sequence[i] = meteorTypesTotal[randomIndex];
            }

            return sequence;
        }

        #endregion
    }
}