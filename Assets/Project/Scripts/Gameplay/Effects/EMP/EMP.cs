using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Effects
{
    public sealed record EMP
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

        public float GetFactor(float timer)
        {
            if (timer < 0f) throw new ArgumentOutOfRangeException();
            return Strength * _strengthOverTime.Evaluate(timer / Duration);
        }
    }
}