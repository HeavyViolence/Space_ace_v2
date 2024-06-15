using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Scatter ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Scatter ammo set config")]
    public sealed class ScatterAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Scatter;

        #region fire rate increase

        public const float MinFireRateIncrease = 0f;
        public const float MaxFireRateIncrease = 20f;

        [SerializeField, MinMaxSlider(MinFireRateIncrease, MaxFireRateIncrease), Space]
        private Vector2 _fireRateIncrease = new(MinFireRateIncrease, MaxFireRateIncrease);

        public Vector2 FireRateIncrease => _fireRateIncrease;

        #endregion

        #region dispersion increase

        public const float MinDispersionIncrease = 0f;
        public const float MaxDispersionIncrease = 10f;

        [SerializeField, MinMaxSlider(MinDispersionIncrease, MaxDispersionIncrease)]
        private Vector2 _dispersionIncrease = new(MinDispersionIncrease, MaxDispersionIncrease);

        public Vector2 DispersionIncrease => _dispersionIncrease;

        #endregion
    }
}