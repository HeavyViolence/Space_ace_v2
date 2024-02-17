using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Nanite missile set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Nanite missile set config")]
    public sealed class NaniteMissileSetConfig : MissileSetConfig
    {
        public override AmmoType AmmoType => AmmoType.NaniteMissiles;

        #region explosion radius

        public const float MinExplosionRadius = 5f;
        public const float MaxExplosionRadius = 20f;

        [SerializeField, MinMaxSlider(MinExplosionRadius, MaxExplosionRadius), Space]
        private Vector2 _explosionRadius = new(MinExplosionRadius, MaxExplosionRadius);

        public Vector2 ExplosionRadius => _explosionRadius;

        #endregion

        #region damage per second

        public const float MinDamagePerSecond = 10f;
        public const float MaxDamagePerSecond = 1000f;

        [SerializeField, MinMaxSlider(MinDamagePerSecond, MaxDamagePerSecond)]
        private Vector2 _damagePerSecond = new(MinDamagePerSecond, MaxDamagePerSecond);

        public Vector2 DamagePerSecond => _damagePerSecond;

        #endregion

        #region damage duration

        public const float MinDamageDuration = 1f;
        public const float MaxDamageDuration = 10f;

        [SerializeField, MinMaxSlider(MinDamageDuration, MaxDamageDuration)]
        private Vector2 _damageDuration = new(MinDamageDuration, MaxDamageDuration);

        public Vector2 DamageDuration => _damageDuration;

        #endregion
    }
}