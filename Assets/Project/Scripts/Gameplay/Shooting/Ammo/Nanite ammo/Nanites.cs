using System;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public readonly struct Nanites
    {
        public float DamagePerSecond { get; }
        public float DamageDuration { get; }

        public Nanites(float damagePerSecond, float damageDuration)
        {
            if (damagePerSecond <= 0f) throw new ArgumentOutOfRangeException();
            if (damageDuration <= 0f) throw new ArgumentOutOfRangeException();

            DamagePerSecond = damagePerSecond;
            DamageDuration = damageDuration;
        }
    }
}