using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Effects
{
    public sealed record Stasis
    {
        public float Slowdown { get; }
        public float SlowdownPercentage => Slowdown * 100f;
        public float Duration { get; }

        private readonly AnimationCurve _slowdownOverTime;

        public Stasis(float slowdown, float duration, AnimationCurve slowdownOverTime)
        {
            if (slowdown < 0f || slowdown > 1f) throw new ArgumentOutOfRangeException();
            if (duration <= 0f) throw new ArgumentOutOfRangeException();
            if (slowdownOverTime == null) throw new ArgumentNullException();

            Slowdown = slowdown;
            Duration = duration;
            _slowdownOverTime = slowdownOverTime;
        }

        public float GetSpeedFactor(float timer)
        {
            if (Slowdown == 1f) return 0f;
            if (Slowdown == 0f) return 1f;

            if (timer < 0f) throw new ArgumentOutOfRangeException();
            return Mathf.Lerp(1f, 1f - Slowdown, _slowdownOverTime.Evaluate(timer / Duration));
        }
    }
}