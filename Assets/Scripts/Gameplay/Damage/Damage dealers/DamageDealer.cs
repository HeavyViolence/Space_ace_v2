using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public abstract class DamageDealer : MonoBehaviour
    {
        public event EventHandler<HitEventArgs> Hit;

        protected virtual void Start()
        {
            if (Hit is null) enabled = false;
        }

        protected virtual void OnDisable()
        {
            Hit = null;
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IDamageable damageReceiver) == true)
                Hit?.Invoke(this, new(transform.position, damageReceiver));
        }
    }
}