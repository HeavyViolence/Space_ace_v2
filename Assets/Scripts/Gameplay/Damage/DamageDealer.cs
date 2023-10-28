using SpaceAce.Gameplay.Damage;

using System;

using UnityEngine;

public sealed class DamageDealer : MonoBehaviour
{
    public event EventHandler<HitEventArgs> Hit;

    private void Start()
    {
        if (Hit is null) enabled = false;
    }

    private void OnDisable()
    {
        Hit = null;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.TryGetComponent(out IDamageable damageReceiver) == true)
            Hit?.Invoke(this, new(transform.position, damageReceiver));
    }
}