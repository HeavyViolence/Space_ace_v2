using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Bombs
{
    [CreateAssetMenu(fileName = "Bomb config",
                     menuName = "Space ace/Configs/Shooting/Bomb config")]
    public sealed class BombConfig : ScriptableObject
    {
        #region explosion damage

        public const float MinExplosionDamage = 0f;
        public const float MaxExplosionDamage = 1_000f;

        [SerializeField, Range(MinExplosionDamage, MaxExplosionDamage)]
        private float _explosionDamage = MinExplosionDamage;

        public float ExplosionDamage => _explosionDamage;

        #endregion

        #region explosion damage falloff

        [SerializeField]
        private AnimationCurve _explosionFalloff;

        public float GetExplosionDamage(float distance)
        {
            if (distance < 0f) throw new ArgumentOutOfRangeException();

            float factor = _explosionFalloff.Evaluate(distance / _explosionRadius);
            return _explosionDamage * factor;
        }

        #endregion

        #region explosion radius

        public const float MinExplosionRadius = 0f;
        public const float MaxExplosionRadius = 100f;

        [SerializeField, Range(MinExplosionDamage, MaxExplosionRadius)]
        private float _explosionRadius = MinExplosionRadius;

        public float ExplosionRadius => _explosionRadius;

        #endregion
    }
}