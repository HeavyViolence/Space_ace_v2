using NaughtyAttributes;

using SpaceAce.Main.Audio;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    [CreateAssetMenu(fileName = "Movement config",
                     menuName = "Space ace/Configs/Movement/Movement config")]
    public class MovementConfig : ScriptableObject
    {
        #region horizontal speed

        public const float MinSpeed = 0f;
        public const float MaxSpeed = 100f;

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _horizontalSpeed = new(MinSpeed, MaxSpeed);

        public float MinHorizontalSpeed => _horizontalSpeed.x;
        public float MaxHorizontalSpeed => _horizontalSpeed.y;
        public float RandomHorizontalSpeed => Random.Range(_horizontalSpeed.x, _horizontalSpeed.y);

        #endregion

        #region vertical speed

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _verticalSpeed = new(MinSpeed, MaxSpeed);

        public float MinVerticalSpeed => _verticalSpeed.x;
        public float MaxVerticalSpeed => _verticalSpeed.y;
        public float RandomVerticalSpeed => Random.Range(_verticalSpeed.x, _verticalSpeed.y);

        #endregion

        #region movement stiffness

        [SerializeField, Space]
        private MovementStiffnessConfig _movementStiffnessConfig;

        public MovementStiffnessConfig MovementStiffness => _movementStiffnessConfig;

        #endregion

        #region viewport bounds displacement

        [SerializeField]
        private BoundsDisplacementConfig _boundsDisplacement;

        public BoundsDisplacementConfig BoundsDisplacement => _boundsDisplacement;

        #endregion

        #region collision damage

        public const float MinCollisionDamage = 0f;
        public const float MaxCollisionDamage = 10000f;

        [SerializeField, MinMaxSlider(MinCollisionDamage, MaxCollisionDamage), Space]
        private Vector2 _collisionDamage = new(MinCollisionDamage, MaxCollisionDamage);

        public float LowestCollisionDamage => _collisionDamage.x;
        public float HighestCollisionDamage => _collisionDamage.y;
        public float RandomCollisionDamage => Random.Range(_collisionDamage.x, _collisionDamage.y);

        [SerializeField]
        private AudioCollection _collisionAudio;
        
        public AudioCollection CollisionAudio => _collisionAudio;

        #endregion
    }
}