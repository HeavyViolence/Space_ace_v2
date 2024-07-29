using SpaceAce.Auxiliary;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Items
{
    public sealed class ItemQualityToSpawnProbabilityConverter
    {
        private readonly Dictionary<Vector2, Quality> _spawnProbabilityInterpolation;

        public ItemQualityToSpawnProbabilityConverter(AnimationCurve spawnProbabilityCurve)
        {
            if (spawnProbabilityCurve == null) throw new ArgumentNullException();
            _spawnProbabilityInterpolation = AuxMath.InterpolateEnumByRange<Quality>(spawnProbabilityCurve);
        }

        public Quality SpawnProbabilityToItemQuality(float spawnProbability)
        {
            if (AuxMath.ValueInRange(0f, 1f, spawnProbability) == false)
                throw new ArgumentOutOfRangeException();

            foreach (var entry in _spawnProbabilityInterpolation)
                if (AuxMath.ValueInRange(entry.Key, spawnProbability) == true)
                    return entry.Value;

            return Quality.Common;
        }

        public Vector2 ItemQualityToSpawnProbability(Quality quality)
        {
            foreach (var entry in _spawnProbabilityInterpolation)
                if (entry.Value == quality)
                    return entry.Key;

            return Vector2.zero;
        }

        public Quality GetProbableQuality(float probabilityFactor = 1f)
        {
            if (probabilityFactor < 0f) throw new ArgumentOutOfRangeException();

            float seed = probabilityFactor == 1f ? AuxMath.RandomNormal
                                                 : Mathf.Clamp01(AuxMath.RandomNormal * probabilityFactor);

            foreach (var entry in _spawnProbabilityInterpolation)
                if (AuxMath.ValueInRange(entry.Key, seed) == true)
                    return entry.Value;

            return Quality.Common;
        }
    }
}