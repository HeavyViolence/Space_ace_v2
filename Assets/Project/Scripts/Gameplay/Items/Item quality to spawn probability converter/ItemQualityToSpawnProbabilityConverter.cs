using SpaceAce.Auxiliary;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Items
{
    public sealed class ItemQualityToSpawnProbabilityConverter
    {
        private readonly AnimationCurve _spawnProbabilityCurve;
        private readonly Dictionary<Vector2, Quality> _spawnProbabilityRangePerItemQuality = new();

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
                float rangeStart = _spawnProbabilityCurve.Evaluate((float)(i - 1) / itemQualities);
                float rangeEnd = _spawnProbabilityCurve.Evaluate((float)i / itemQualities);

                Vector2 range = new(rangeStart, rangeEnd);
                Quality quality = Enum.Parse<Quality>(qualityNames[i - 1], true);

                _spawnProbabilityRangePerItemQuality.Add(range, quality);
            }
        }

        public Quality SpawnProbabilityToItemQuality(float spawnProbability)
        {
            if (AuxMath.ValueInRange(0f, 1f, spawnProbability) == false)
                throw new ArgumentOutOfRangeException();

            foreach (var entry in _spawnProbabilityRangePerItemQuality)
                if (AuxMath.ValueInRange(entry.Key, spawnProbability) == true)
                    return entry.Value;

            return Quality.Common;
        }

        public Vector2 ItemQualityToSpawnProbability(Quality quality)
        {
            foreach (var entry in _spawnProbabilityRangePerItemQuality)
                if (entry.Value == quality)
                    return entry.Key;

            return Vector2.zero;
        }

        public Quality GetProbableQuality()
        {
            float seed = AuxMath.RandomNormal;

            foreach (var entry in _spawnProbabilityRangePerItemQuality)
                if (AuxMath.ValueInRange(entry.Key, seed) == true)
                    return entry.Value;

            return Quality.Common;
        }
    }
}