using SpaceAce.Auxiliary;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Items
{
    public sealed class ItemQualityToSpawnProbabilityConverter
    {
        private readonly AnimationCurve _spawnProbabilityCurve;
        private readonly Dictionary<Quality, Vector2> _spawnProbabilityRangePerItemQuality = new();

        public ItemQualityToSpawnProbabilityConverter(AnimationCurve spawnProbabilityCurve)
        {
            if (spawnProbabilityCurve == null) throw new ArgumentNullException();
            _spawnProbabilityCurve = spawnProbabilityCurve;

            BuildSpawnProbabilityRangePerItemQuality();
        }

        private void BuildSpawnProbabilityRangePerItemQuality()
        {
            int itemQualities = Enum.GetValues(typeof(Quality)).Length;
            string[] qualityNames = Enum.GetNames(typeof(Quality));

            for (int i = 1; i <= itemQualities; i++)
            {
                float rangeStartEvaluator = (float)(i - 1) / itemQualities;
                float rangeEndEvaluator = (float)i / itemQualities;

                float rangeStart = _spawnProbabilityCurve.Evaluate(rangeStartEvaluator);
                float rangeEnd = _spawnProbabilityCurve.Evaluate(rangeEndEvaluator);

                Vector2 range = new(rangeStart, rangeEnd);
                Quality quality = Enum.Parse<Quality>(qualityNames[i - 1], true);

                _spawnProbabilityRangePerItemQuality.Add(quality, range);
            }
        }

        public Quality SpawnProbabilityToItemQuality(float spawnProbability)
        {
            if (AuxMath.ValueInRange(0f, 1f, spawnProbability) == false)
                throw new ArgumentOutOfRangeException();

            foreach (var cache in _spawnProbabilityRangePerItemQuality)
                if (AuxMath.ValueInRange(cache.Value, spawnProbability) == true)
                    return cache.Key;

            return Quality.Common;
        }

        public Vector2 ItemQualityToSpawnProbability(Quality quality) =>
            _spawnProbabilityRangePerItemQuality[quality];
    }
}