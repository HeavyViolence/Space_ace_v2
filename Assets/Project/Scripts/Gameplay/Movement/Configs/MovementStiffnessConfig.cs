using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    [CreateAssetMenu(fileName = "Movement stiffness config",
                     menuName = "Space ace/Configs/Movement/Movement stiffness config")]
    public sealed class MovementStiffnessConfig : ScriptableObject
    {
        public const float MinStiffness = 0.1f;
        public const float MaxStiffness = 10f;

        [SerializeField, MinMaxSlider(MinStiffness, MaxStiffness), Space]
        private Vector2 _movementStiffness = new(MinStiffness, MaxStiffness);

        public float MinMovementStiffness => _movementStiffness.x;
        public float MaxMovementStiffness => _movementStiffness.y;
        public float RandomMovementStiffness => Random.Range(_movementStiffness.x, _movementStiffness.y);

        [SerializeField, MinMaxSlider(MinStiffness, MaxStiffness)]
        private Vector2 _brakingStiffness = new(MinStiffness, MaxStiffness);

        public float MinBrakingStiffness => _brakingStiffness.x;
        public float MaxBrakingStiffness => _brakingStiffness.y;
        public float RandomBrakingStiffness => Random.Range(_brakingStiffness.x, _brakingStiffness.y);

        [SerializeField, MinMaxSlider(MinStiffness, MaxStiffness)]
        private Vector2 _viewportReboundStiffness = new(MinStiffness, MaxStiffness);

        public float MinViewportReboundStiffness => _viewportReboundStiffness.x;
        public float MaxViewportReboundStiffness => _viewportReboundStiffness.y;
        public float RandomViewportReboundStiffness => Random.Range(_viewportReboundStiffness.x, _viewportReboundStiffness.y);

        [SerializeField, MinMaxSlider(MinStiffness, MaxStiffness)]
        private Vector2 _rotationStiffness = new(MinStiffness, MaxStiffness);

        public float MinRotationStiffness => _rotationStiffness.x;
        public float MaxRotationStiffness => _rotationStiffness.y;
        public float RandomRotationStiffness => Random.Range(_rotationStiffness.x, _rotationStiffness.y);
    }
}