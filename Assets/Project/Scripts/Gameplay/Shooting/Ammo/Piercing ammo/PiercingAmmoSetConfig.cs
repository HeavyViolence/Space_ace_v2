using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Piercing ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Piercing ammo set config")]
    public sealed class PiercingAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Piercing;

        #region projectile hits

        public const int MinProjectileHits = 2;
        public const int MaxProjectileHits = 10;

        [SerializeField, MinMaxSlider(MinProjectileHits, MaxProjectileHits), Space]
        private Vector2Int _projectileHits = new(MinProjectileHits, MaxProjectileHits);

        public Vector2Int ProjectileHits => _projectileHits;

        #endregion

        #region heat generation factor

        public const float MinHeatGenerationFactorPerShot = 1f;
        public const float MaxHeatGenerationFactorPerShot = 2f;

        [SerializeField, MinMaxSlider(MinHeatGenerationFactorPerShot, MaxHeatGenerationFactorPerShot)]
        private Vector2 _heatGenerationFactorPerShot = new(MinHeatGenerationFactorPerShot, MaxHeatGenerationFactorPerShot);

        public Vector2 HeatGenerationFactorPerShot => _heatGenerationFactorPerShot;

        #endregion
    }
}