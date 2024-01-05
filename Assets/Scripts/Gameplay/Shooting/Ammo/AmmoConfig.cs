using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main.Audio;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public abstract class AmmoConfig : ScriptableObject
    {
        [SerializeField]
        private AudioCollection _shotAudio;

        public AudioCollection ShotAudio => _shotAudio;

        #region price

        public const float MinBasePrice = 0.01f;
        public const float MaxBasePrice = 100f;

        [SerializeField, Range(MinBasePrice, MaxBasePrice), Space]
        private float _basePrice = MinBasePrice;

        [SerializeField, Range(1f, 2f)]
        private float _basePriceFactorPerSize = 1f;

        [SerializeField, Range(1f, 2f)]
        private float _basePriceFactorPerQuality = 1f;

        public float GetPrice(ItemSize size, ItemQuality quality)
        {
            float modifiedBySize = AuxMath.ModifyItemPropertyBySize(_basePrice, _basePriceFactorPerSize, size);
            float modifiedBySizeAndQuality = AuxMath.ModifyItemPropertyByQuality(modifiedBySize, _basePriceFactorPerQuality, quality);

            return modifiedBySizeAndQuality;
        }

        #endregion

        #region heat generation

        public const float MinBaseHeatGeneration = 1f;
        public const float MaxBaseHeatGeneration = 100f;

        [SerializeField, Range(MinBaseHeatGeneration, MaxBaseHeatGeneration), Space]
        private float _baseHeatGeneration = MinBaseHeatGeneration;

        [SerializeField, Range(1f, 2f)]
        private float _heatGenerationFactorPerSize = 1f;

        [SerializeField, Range(0f, 1f)]
        private float _heatGenerationFactorPerQuality = 0f;

        public float GetHeatGeneration(ItemSize size, ItemQuality quality)
        {
            float modifiedBySize = AuxMath.ModifyItemPropertyBySize(_baseHeatGeneration, _heatGenerationFactorPerSize, size);
            float modifiedBySizeAndQuality = AuxMath.ModifyItemPropertyByQuality(modifiedBySize, _heatGenerationFactorPerQuality, quality);

            return modifiedBySizeAndQuality;
        }

        #endregion

        #region speed

        public const float MinBaseSpeed = 50f;
        public const float MaxBaseSpeed = 200f;

        [SerializeField, Range(MinBaseSpeed, MaxBaseSpeed), Space]
        private float _baseSpeed = MinBaseSpeed;

        [SerializeField, Range(0f, 1f)]
        private float _speedFactorPerSize = 0f;

        [SerializeField, Range(1f, 2f)]
        private float _speedFactorPerQuality = 1f;

        public float GetSpeed(ItemSize size, ItemQuality quality)
        {
            float modifiedBySize = AuxMath.ModifyItemPropertyBySize(_baseSpeed, _speedFactorPerSize, size);
            float modifiedBySizeAndQuality = AuxMath.ModifyItemPropertyByQuality(modifiedBySize, _speedFactorPerQuality, quality);

            return modifiedBySizeAndQuality;
        }

        #endregion

        #region damage

        public const float MinBaseDamage = 10f;
        public const float MaxBaseDamage = 1000f;

        [SerializeField, Range(MinBaseDamage, MaxBaseDamage), Space]
        private float _baseDamage = MinBaseDamage;

        [SerializeField, Range(1f, 2f)]
        private float _damageFactorPerSize = 1f;

        [SerializeField, Range(1f, 2f)]
        private float _damageFactorPerQuality = 1f;

        public float GetDamage(ItemSize size, ItemQuality quality)
        {
            float modifiedBySize = AuxMath.ModifyItemPropertyBySize(_baseDamage, _damageFactorPerSize, size);
            float modifiedBySizeAndQuality = AuxMath.ModifyItemPropertyByQuality(modifiedBySize, _damageFactorPerQuality, quality);

            return modifiedBySizeAndQuality;
        }

        #endregion
    }
}