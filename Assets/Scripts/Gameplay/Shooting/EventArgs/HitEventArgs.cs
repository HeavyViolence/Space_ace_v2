using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting
{
    public sealed class HitEventArgs : EventArgs
    {
        public Vector3 HitPosition { get; }
        public IDamageable DamageReceiver { get; }

        public HitEventArgs(Vector3 hitPosition,
                            IDamageable damageReceiver)
        {
            HitPosition = hitPosition;
            DamageReceiver = damageReceiver;
        }
    }
}