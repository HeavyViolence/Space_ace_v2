using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Catalytic ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Catalytic ammo set config")]
    public sealed class CatalyticAmmoSetConfig : AmmoSetConfig
    {
        public const float MinFireRateFactorPerShot = 1f;
        public const float MaxFireRateFactorPerShot = 2f;

        [SerializeField, MinMaxSlider(MinFireRateFactorPerShot, MaxFireRateFactorPerShot), Space]
        private Vector2 _fireRateFactorPerShot = new(MinFireRateFactorPerShot, MaxFireRateFactorPerShot);

        public Vector2 FireRateFactorPerShot => _fireRateFactorPerShot;

        public override AmmoType AmmoType => AmmoType.Catalytic;
    }
}