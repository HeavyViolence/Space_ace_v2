using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class HitEventArgs : EventArgs
    {
        public Vector3 HitPosition { get; }
        public IDamageable Damageable { get; }
        public GameObject ObjectBeingHit { get; }

        public HitEventArgs(Vector3 hitPosition,
                            IDamageable damageable,
                            GameObject objectBeingHit)
        {
            HitPosition = hitPosition;
            Damageable = damageable;
            ObjectBeingHit = objectBeingHit;
        }
    }
}