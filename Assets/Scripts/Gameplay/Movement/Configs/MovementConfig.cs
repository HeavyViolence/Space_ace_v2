using NaughtyAttributes;

using SpaceAce.Auxiliary;
using SpaceAce.Main.Audio;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    [CreateAssetMenu(fileName = "Movement config",
                     menuName = "Space ace/Configs/Movement/Movement config")]
    public class MovementConfig : ScriptableObject
    {
        public const float MinSpeed = 0f;
        public const float MaxSpeed = 100f;

        public const float MaxRotationSpeed = 60f;

        public const float MinBoundDisplacement = -1f;
        public const float MaxBoundDisplacement = 2f;
        public const float DefaultBoundDisplacement = 1f;

        public const float MinCollisionDamage = 0f;
        public const float MaxCollisionDamage = 10000f;

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _horizontalSpeed = new(MinSpeed, MaxSpeed);

        public float MinHorizontalSpeed => _horizontalSpeed.x;
        public float MaxHorizontalSpeed => _horizontalSpeed.y;
        public float RandomHorizontalSpeed => Random.Range(_horizontalSpeed.x, _horizontalSpeed.y);

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _verticalSpeed = new(MinSpeed, MaxSpeed);

        public float MinVerticalSpeed => _verticalSpeed.x;
        public float MaxVerticalSpeed => _verticalSpeed.y;
        public float RandomVerticalSpeed => Random.Range(_verticalSpeed.x, _verticalSpeed.y);

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _spatialSpeed = new(MinSpeed, MaxSpeed);

        public float MinSpatialSpeed => _spatialSpeed.x;
        public float MaxSpatialSpeed => _spatialSpeed.y;
        public float RandomSpatialSpeed => Random.Range(_spatialSpeed.x, _spatialSpeed.y);

        [SerializeField, MinMaxSlider(0f, MaxRotationSpeed)]
        private Vector2 _rotationSpeed = new(0f, MaxRotationSpeed);

        public float LowestRotationSpeed => _rotationSpeed.x;
        public float HighestRotationSpeed => _rotationSpeed.y;
        public float RandomRotationSpeed => AuxMath.RandomSign * Random.Range(_rotationSpeed.x, _rotationSpeed.y);

        [SerializeField, MinValue(MinBoundDisplacement), MaxValue(MaxBoundDisplacement), HorizontalLine]
        private float _upperBoundDisplacement = DefaultBoundDisplacement;

        [SerializeField, MinValue(MinBoundDisplacement), MaxValue(MaxBoundDisplacement)]
        private float _lowerBoundDisplacement = DefaultBoundDisplacement;

        [SerializeField, MinValue(MinBoundDisplacement), MaxValue(MaxBoundDisplacement)]
        private float _leftBoundDisplacement = DefaultBoundDisplacement;

        [SerializeField, MinValue(MinBoundDisplacement), MaxValue(MaxBoundDisplacement)]
        private float _rightBoundDisplacement = DefaultBoundDisplacement;

        public float UpperBoundDisplacement => _upperBoundDisplacement;
        public float LowerBoundDisplacement => _lowerBoundDisplacement;
        public float LeftBoundDisplacement => _leftBoundDisplacement;
        public float RightBoundDisplacement => _rightBoundDisplacement;

        [SerializeField, MinMaxSlider(MinCollisionDamage, MaxCollisionDamage), HorizontalLine]
        private Vector2 _collisionDamage = new(MinCollisionDamage, MaxCollisionDamage);

        public float LowestCollisionDamage => _collisionDamage.x;
        public float HighestCollisionDamage => _collisionDamage.y;
        public float RandomCollisionDamage => Random.Range(_collisionDamage.x, _collisionDamage.y);

        [SerializeField]
        private AudioCollection _collisionAudio;
        
        public AudioCollection CollisionAudio => _collisionAudio;
    }
}