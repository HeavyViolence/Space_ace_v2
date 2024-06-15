using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Antimatter ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Antimatter ammo set config")]
    public sealed class AntimatterAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Antimatter;

        #region damage increase per shot

        public const float MinDamageFactorPerShot = 1f;
        public const float MaxDamageFactorPerShot = 10f;

        [SerializeField, MinMaxSlider(MinDamageFactorPerShot, MaxDamageFactorPerShot), Space]
        private Vector2 _damageFactorPerShot = new(MinDamageFactorPerShot, MaxDamageFactorPerShot);

        public Vector2 DamageFactorPerShot => _damageFactorPerShot;

        #endregion

        #region fire rate decrease per shot

        public const float MinFireRateFactorPerShot = 0f;
        public const float MaxFireRateFactorPerShot = 1f;

        [SerializeField, MinMaxSlider(MinFireRateFactorPerShot, MaxFireRateFactorPerShot)]
        private Vector2 _fireRateFactorPerShot = new(MinFireRateFactorPerShot, MaxFireRateFactorPerShot);

        public Vector2 FireRateFactorPerShot => _fireRateFactorPerShot;

        #endregion
    }
}