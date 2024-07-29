using SpaceAce.Auxiliary;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "Enemy mutation config",
                     menuName = "Space ace/Configs/Enemies/Enemy mutation config")]
    public sealed class EnemyMutationConfig : ScriptableObject
    {
        public const float MinMutationProbabilityPerLevel = 0f;
        public const float MaxMutationProbabilityPerLevel = 1f;

        [SerializeField]
        private List<EnemyMutation> _mutationsByRarity;

        [SerializeField, Space]
        private AnimationCurve _mutationsProbability;

        [SerializeField, Range(MinMutationProbabilityPerLevel, MaxMutationProbabilityPerLevel)]
        private float _mutationProbabilityPerLevel = MinMutationProbabilityPerLevel;

        private Dictionary<Vector2, EnemyMutation> _mutationsDistribution;

        public EnemyMutation GetProbableMutation(int level)
        {
            if (level <= 0) throw new ArgumentOutOfRangeException();

            if (AuxMath.RandomNormal > Mathf.Clamp01(_mutationProbabilityPerLevel * level))
                return EnemyMutation.None;

            float seed = AuxMath.RandomNormal;

            foreach (var entry in _mutationsDistribution)
                if (AuxMath.ValueInRange(entry.Key, seed) == true)
                    return entry.Value;

            return EnemyMutation.None;
        }

        private void OnEnable()
        {
            _mutationsDistribution = AuxMath.InterpolateEnumByRange(_mutationsProbability, _mutationsByRarity);
        }
    }
}