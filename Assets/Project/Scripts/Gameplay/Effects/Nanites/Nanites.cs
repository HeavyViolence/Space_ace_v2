using System;

namespace SpaceAce.Gameplay.Effects
{
    public readonly struct Nanites
    {
        public float DamagePerSecond { get; }
        public float Duration { get; }

        public Nanites(float damagePerSecond, float duration)
        {
            if (damagePerSecond <= 0f) throw new ArgumentOutOfRangeException();
            if (duration <= 0f) throw new ArgumentOutOfRangeException();

            DamagePerSecond = damagePerSecond;
            Duration = duration;
        }
    }
}