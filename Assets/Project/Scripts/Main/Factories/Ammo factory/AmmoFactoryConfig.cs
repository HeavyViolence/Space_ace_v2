using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Shooting.Ammo;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories.AmmoFactories
{
    [CreateAssetMenu(fileName = "Ammo factory config",
                     menuName = "Space ace/Configs/Factories/Ammo factory config")]
    public sealed class AmmoFactoryConfig : ScriptableObject
    {
        private readonly Dictionary<Vector2, AmmoType> _ammoTypesSpawnProbabilityDistribution = new();

        [SerializeField]
        private List<AmmoSetConfig> _ammoConfigs;

        [SerializeField]
        private AnimationCurve _ammoSpawnProbabilityByPrice;

        private void OnEnable()
        {
            if (_ammoConfigs is not null && _ammoConfigs.Count > 1)
            {
                _ammoConfigs.Sort(SortAmmoConfigsByPriceAscending);
                BuildAmmoTypesSpawnProbabilityDistribution();
            }
        }

        private int SortAmmoConfigsByPriceAscending(AmmoSetConfig c1, AmmoSetConfig c2)
        {
            if (c1.Price.x + c1.Price.y > c2.Price.x + c2.Price.y) return 1;
            if (c1.Price.x + c1.Price.y < c2.Price.x + c2.Price.y) return -1;

            return 0;
        }

        private void BuildAmmoTypesSpawnProbabilityDistribution()
        {
            List<AmmoType> ammoTypesValues = new(_ammoConfigs.Count);

            foreach (AmmoSetConfig config in _ammoConfigs)
                ammoTypesValues.Add(config.AmmoType);

            for (int i = _ammoConfigs.Count; i > 0; i--)
            {
                float rangeStart = _ammoSpawnProbabilityByPrice.Evaluate((float)(i - 1) / _ammoConfigs.Count);
                float rangeEnd = _ammoSpawnProbabilityByPrice.Evaluate((float)i / _ammoConfigs.Count);

                Vector2 probabilityRange = new(rangeStart, rangeEnd);
                AmmoType type = ammoTypesValues[i - 1];

                _ammoTypesSpawnProbabilityDistribution.Add(probabilityRange, type);
            }
        }

        public T GetAmmoConfig<T>() where T : AmmoSetConfig
        {
            foreach (var config in _ammoConfigs)
                if (config.GetType() == typeof(T))
                    return (T)config;

            throw new Exception($"{nameof(AmmoFactoryConfig)} is missing ammo config of type {typeof(T)}!");
        }

        public AmmoType GetProbableAmmoType()
        {
            float seed = AuxMath.RandomNormal;

            foreach (var entry in _ammoTypesSpawnProbabilityDistribution)
                if (AuxMath.ValueInRange(entry.Key, seed) == true)
                    return entry.Value;

            return AmmoType.Regular;
        }
    }
}