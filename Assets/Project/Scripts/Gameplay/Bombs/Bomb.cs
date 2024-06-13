using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Experience;

using UnityEngine;

namespace SpaceAce.Gameplay.Bombs
{
    [RequireComponent(typeof(Durability))]
    public sealed class Bomb : MonoBehaviour, IExperienceSource
    {
        private static int s_damageMask;

        [SerializeField]
        private BombConfig _config;

        private Durability _durability;
        private Transform _transform;

        public float ExplosionDamage => _config.ExplosionDamage;
        public float ExplosionRadius => _config.ExplosionRadius;

        private void Awake()
        {
            s_damageMask = LayerMask.GetMask("Player", "Enemies", "Bosses", "Meteors", "Wrecks", "Bombs");

            _durability = GetComponent<Durability>();
            _transform = transform;
        }

        private void OnDisable()
        {
            DealExplosionDamageIfDestroyed();
        }

        private void DealExplosionDamageIfDestroyed()
        {
            if (_durability.Value > 0f) return;

            RaycastHit2D[] hits = Physics2D.CircleCastAll(_transform.position, ExplosionRadius, Vector2.zero, float.PositiveInfinity, s_damageMask);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject.TryGetComponent(out DamageReceiver damageReceiver) == true)
                {
                    float explosionDamage = _config.GetExplosionDamage(hit.distance);
                    damageReceiver.ApplyDamage(explosionDamage);
                }
            }
        }

        public float GetExperience() => ExplosionDamage;
    }
}