using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class DamageDealer : MonoBehaviour
    {
        public event EventHandler<HitEventArgs> Hit;

        private Transform _transform;

        public int HitCount { get; private set; } = 0;

        private void Awake()
        {
            _transform = transform;
        }

        private void OnDisable()
        {
            Hit = null;
            HitCount = 0;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IDamageable damageable) == true)
            {
                HitCount++;
                Hit?.Invoke(this, new(_transform.position, damageable, collider.gameObject));
            }
        }
    }
}