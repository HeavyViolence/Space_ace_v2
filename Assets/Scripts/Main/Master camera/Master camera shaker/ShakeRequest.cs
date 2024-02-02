using System;

using UnityEngine;

namespace SpaceAce.Main
{
    public class ShakeRequest : IEquatable<ShakeRequest>
    {
        public Guid ID { get; }

        public float Amplitude { get; }
        public float CurrentAmplitude { get; private set; }

        public float Frequency { get; }
        public float CurrentFrequency { get; private set; }

        public float Duration { get; }
        public float CurrentDuration { get; private set; }
        public float NormalizedDuration => CurrentDuration / Duration;

        public ShakeRequest(float amplitude, float frequency, float duration)
        {
            ID = Guid.NewGuid();

            Amplitude = amplitude;
            CurrentAmplitude = 0f;

            Frequency = frequency;
            CurrentFrequency = 0f;

            Duration = duration;
            CurrentDuration = 0f;
        }

        public void FixedUpdate(AnimationCurve shakeCurve)
        {
            CurrentDuration += Time.fixedDeltaTime;
            CurrentAmplitude = Amplitude * shakeCurve.Evaluate(NormalizedDuration);
            CurrentFrequency = Frequency * shakeCurve.Evaluate(NormalizedDuration);
        }

        #region interfaces

        public override bool Equals(object obj) => obj is not null &&
                                                       obj is ShakeRequest request &&
                                                       Equals(request);

        public bool Equals(ShakeRequest other) => ID == other.ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion
    }
}