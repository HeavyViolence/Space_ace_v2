using NaughtyAttributes;

using SpaceAce.Architecture;
using SpaceAce.Main;
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

        public const float MinRotationSpeed = -60f;
        public const float MaxRotationSpeed = 60f;

        public const float MinBoundDisplacement = 0f;
        public const float MaxBoundDisplacement = 2f;
        public const float DefaultBoundDisplacement = 1f;

        public const float MinCollisionDamage = 0f;
        public const float MaxCollisionDamage = 10000f;

        private static readonly CachedService<MasterCameraHolder> s_masterCameraHolder = new();

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _horizontalSpeed = new(MinSpeed, MaxSpeed);

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _verticalSpeed = new(MinSpeed, MaxSpeed);

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _spatialSpeed = new(MinSpeed, MaxSpeed);

        [SerializeField, MinMaxSlider(MinRotationSpeed, MaxRotationSpeed)]
        private Vector2 _rotationSpeed = new(MinRotationSpeed, MaxRotationSpeed);

        [SerializeField, MinValue(MinBoundDisplacement), MaxValue(MaxBoundDisplacement), HorizontalLine]
        private float _upperBoundDisplacement = DefaultBoundDisplacement;

        [SerializeField, MinValue(MinBoundDisplacement), MaxValue(MaxBoundDisplacement)]
        private float _lowerBoundDisplacement = DefaultBoundDisplacement;

        [SerializeField, MinValue(MinBoundDisplacement), MaxValue(MaxBoundDisplacement)]
        private float _leftBoundDisplacement = DefaultBoundDisplacement;

        [SerializeField, MinValue(MinBoundDisplacement), MaxValue(MaxBoundDisplacement)]
        private float _rightBoundDisplacement = DefaultBoundDisplacement;

        [SerializeField, MinMaxSlider(MinCollisionDamage, MaxCollisionDamage), HorizontalLine]
        private Vector2 _collisionDamage = new(MinCollisionDamage, MaxCollisionDamage);

        [SerializeField]
        private AudioCollection _collisionAudio = null;

        [SerializeField]
        private bool _cameraShakeOnCollision = false;

        public float MinHorizontalSpeed => _horizontalSpeed.x;
        public float MaxHorizontalSpeed => _horizontalSpeed.y;

        public float MinVerticalSpeed => _verticalSpeed.x;
        public float MaxVerticalSpeed => _verticalSpeed.y;

        public float MinSpatialSpeed => _spatialSpeed.x;
        public float MaxSpatialSpeed => _spatialSpeed.y;

        public float LowestRotationSpeed => _rotationSpeed.x;
        public float HighestRotationSpeed => _rotationSpeed.y;

        public float UpperBound => s_masterCameraHolder.Access.ViewportUpperBound * _upperBoundDisplacement;
        public float LowerBound => s_masterCameraHolder.Access.ViewportLowerBound * _lowerBoundDisplacement;
        public float LeftBound => s_masterCameraHolder.Access.ViewportLeftBound * _leftBoundDisplacement;
        public float RightBound => s_masterCameraHolder.Access.ViewportRightBound * _rightBoundDisplacement;

        public float LowestCollisionDamage => _collisionDamage.x;
        public float HighestCollisionDamage => _collisionDamage.y;

        public AudioCollection CollisionAudio => _collisionAudio;

        public bool CameraShakeOnCollisionEnabled => _cameraShakeOnCollision;
    }
}