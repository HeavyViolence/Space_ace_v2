using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class DamageDealer : MonoBehaviour
    {
        public event EventHandler<HitEventArgs> Hit;

        public int HitCount { get; private set; } = 0;

        private void OnDisable()
        {
            Hit = null;
            HitCount = 0;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IDamageable damageReceiver) == true)
            {
                HitCount++;
                Hit?.Invoke(this, new(transform.position, damageReceiver, collider.gameObject));
            }
        }
    }
}