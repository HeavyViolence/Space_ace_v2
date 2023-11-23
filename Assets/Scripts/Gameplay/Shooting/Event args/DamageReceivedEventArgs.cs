using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting
{
    public sealed class DamageReceivedEventArgs : EventArgs
    {
        public float DamageReceived { get; }
        public float DamageTaken { get; }
        public float DamageLost => DamageReceived - DamageTaken;
        public float DamageEfficiency => DamageTaken / DamageReceived;
        public Vector2 HitPosition { get; }

        public DamageReceivedEventArgs(float damageReceived,
                                       float damageTaken,
                                       Vector2 hitPosition)
        {
            DamageReceived = damageReceived;
            DamageTaken = damageTaken;
            HitPosition = hitPosition;
        }
    }
}