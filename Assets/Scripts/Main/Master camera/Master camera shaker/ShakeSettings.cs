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

        public const float MinAttenuation = 0.1f;
        public const float MaxAttenuation = 10f;

        public const float MinFrequency = 0.1f;
        public const float MaxFrequency = 10f;

        public const float AmplitudeCutoff = 0.01f;

        public static ShakeSettings Default => new(true, MinAmplitude, MinAttenuation, MinFrequency);

        [SerializeField, JsonIgnore]
        private bool _enabled = false;

        [SerializeField, Range(MinAmplitude, MaxAmplitude), JsonIgnore]
        private float _amplitude = MinAmplitude;

        [SerializeField, Range(MinAttenuation, MaxAttenuation), JsonIgnore]
        private float _attenuation = MinAttenuation;

        [SerializeField, Range(MinFrequency, MaxFrequency), JsonIgnore]
        private float _frequency = MinFrequency;

        public bool Enabled => _enabled;
        public float Amplitude => _amplitude;
        public float Attenuation => _attenuation;
        public float Frequency => _frequency;

        public ShakeSettings(bool enabled, float amplitude, float attenuation, float frequency)
        {
            _enabled = enabled;
            _amplitude = Mathf.Clamp(amplitude, MinAmplitude, MaxAmplitude);
            _attenuation = Mathf.Clamp(attenuation, MinAttenuation, MaxAttenuation);
            _frequency = Mathf.Clamp(frequency, MinFrequency, MaxFrequency);
        }
    }
}