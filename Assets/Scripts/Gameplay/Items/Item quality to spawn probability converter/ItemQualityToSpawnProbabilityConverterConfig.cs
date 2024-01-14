using NaughtyAttributes;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Items
{
    [CreateAssetMenu(fileName = "Item quality to spawn probability converter config",
                     menuName = "Space ace/Configs/Items/Item quality to spawn probability converter config")]
    public sealed class ItemQualityToSpawnProbabilityConverterConfig : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve _spawnProbabilityCurve;

        public AnimationCurve SpawnProbabilityCurve => _spawnProbabilityCurve;

        [Button]
        private void LogSpawnProbabilities()
        {
            int qualityAmount = Enum.GetValues(typeof(Quality)).Length;
            string[] qualityNames = Enum.GetNames(typeof(Quality));

            for (int i = 0; i < qualityAmount; i++)
            {
                float evaluator = (float)i / qualityAmount;
                float spawnProbability = _spawnProbabilityCurve.Evaluate(evaluator);

                Debug.Log($"{qualityNames[i]} -> {spawnProbability:n2}");
            }
        }
    }
}