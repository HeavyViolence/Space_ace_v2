using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    [CreateAssetMenu(fileName = "Bounds displacement config",
                     menuName = "Space ace/Configs/Movement/Bounds displacement config")]
    public sealed class BoundsDisplacementConfig : ScriptableObject
    {
        public const float MinBoundDisplacement = -1f;
        public const float MaxBoundDisplacement = 2f;

        [SerializeField, Range(MinBoundDisplacement, MaxBoundDisplacement), Space]
        private float _upperBoundDisplacement = 1f;

        public float UpperBoundDisplacement => _upperBoundDisplacement;

        [SerializeField, Range(MinBoundDisplacement, MaxBoundDisplacement)]
        private float _lowerBoundDisplacement = 1f;

        public float LowerBoundDisplacement => _lowerBoundDisplacement;

        [SerializeField, Range(MinBoundDisplacement, MaxBoundDisplacement)]
        private float _sideBoundsDisplacement = 1f;

        public float SideBoundsDisplacement => _sideBoundsDisplacement;
    }
}