using Newtonsoft.Json;

using System;

using UnityEngine;

namespace SpaceAce.Main
{
    [Serializable]
    public sealed class ShakeSettings
    {
        public const float MinAmplitude = 0.01f;
        public const float MaxAmplitude = 1f;

        public const float MinFrequency = 0.1f;
        public const float MaxFrequency = 100f;

        public const float MinDuration = 0.1f;
        public const float MaxDuration = 10f;

        public static ShakeSettings Default => new(true, MinAmplitude, MinFrequency, MinDuration);

        [SerializeField, JsonIgnore]
        private bool _enabled = false;

        [SerializeField, Range(MinAmplitude, MaxAmplitude), JsonIgnore]
        private float _amplitude = MinAmplitude;

        [SerializeField, Range(MinFrequency, MaxFrequency), JsonIgnore]
        private float _frequency = MinFrequency;

        [SerializeField, Range(MinDuration, MaxDuration), JsonIgnore]
        private float _duration = MinDuration;

        public bool Enabled => _enabled;
        public float Amplitude => _amplitude;
        public float Frequency => _frequency;
        public float Duration => _duration;

        [JsonIgnore]
        public ShakeRequest NewShakeRequest => new(Amplitude, Frequency, Duration);

        public ShakeSettings(bool enabled,
                             float amplitude,
                             float frequency,
                             float duration)
        {
            _enabled = enabled;
            _amplitude = Mathf.Clamp(amplitude, MinAmplitude, MaxAmplitude);
            _frequency = Mathf.Clamp(frequency, MinFrequency, MaxFrequency);
            _duration = Mathf.Clamp(duration, MinDuration, MaxDuration);
        }
    }
}