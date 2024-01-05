using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class DamageDealer : MonoBehaviour
    {
        public event EventHandler<HitEventArgs> Hit;

        private void OnDisable()
        {
            Hit = null;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IDamageable damageReceiver) == true)
                Hit?.Invoke(this, new(transform.position, damageReceiver, collider.gameObject));
        }
    }
}