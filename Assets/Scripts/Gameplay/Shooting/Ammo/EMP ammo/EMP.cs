using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public readonly struct EMP
    {
        public float Strength { get; }
        public float StrengthPercentage => Strength * 100f;

        public float Duration { get; }

        private readonly AnimationCurve _strengthOverTime;

        public EMP(float strength, float duration, AnimationCurve strengthOverTime)
        {
            if (strength <= 0f || strength > 1f) throw new ArgumentOutOfRangeException();
            if (duration <= 0f) throw new ArgumentOutOfRangeException();
            if (strengthOverTime == null) throw new ArgumentNullException();

            Strength = strength;
            Duration = duration;
            _strengthOverTime = strengthOverTime;
        }

        public float GetCurrentFactor(float timer)
        {
            if (timer < 0f) throw new ArgumentOutOfRangeException();
            if (timer >= 1f) return 1f;

            return 1f - Strength * _strengthOverTime.Evaluate(timer / Duration);
        }
    }
}