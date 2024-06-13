using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Explosive ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Explosive ammo set config")]
    public sealed class ExplosiveAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Explosive;

        #region explosion radius

        public const float MinExplosionRadius = 5f;
        public const float MaxExplosionRadius = 20f;

        [SerializeField, MinMaxSlider(MinExplosionRadius, MaxExplosionRadius), Space]
        private Vector2 _explosionRadius = new(MinExplosionRadius, MaxExplosionRadius);

        public Vector2 ExplosionRadius => _explosionRadius;

        #endregion

        #region explosion damage

        public const float MinExplosionDamage = 10f;
        public const float MaxExplosionDamage = 1000f;

        [SerializeField, MinMaxSlider(MinExplosionDamage, MaxExplosionDamage)]
        private Vector2 _explosionDamage = new(MinExplosionDamage, MaxExplosionDamage);

        public Vector2 ExplosionDamage => _explosionDamage;

        #endregion
    }
}