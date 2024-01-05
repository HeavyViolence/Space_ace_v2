using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class HitEventArgs : EventArgs
    {
        public Vector3 HitPosition { get; }
        public IDamageable DamageReceiver { get; }
        public GameObject ObjectBeingHit { get; }

        public HitEventArgs(Vector3 hitPosition,
                            IDamageable damageReceiver,
                            GameObject objectBeingHit)
        {
            HitPosition = hitPosition;
            DamageReceiver = damageReceiver;
            ObjectBeingHit = objectBeingHit;
        }
    }
}