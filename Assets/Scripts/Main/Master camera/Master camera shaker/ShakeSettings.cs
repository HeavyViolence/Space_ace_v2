using NaughtyAttributes;

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

        public static ShakeSettings Default => new(false, Vector2.zero, Vector2.zero, Vector2.zero);

        [SerializeField]
        private bool _enabled;

        [SerializeField, MinMaxSlider(MinAmplitude, MaxAmplitude)]
        private Vector2 _amplitude;

        [SerializeField, MinMaxSlider(MinAttenuation, MaxAttenuation)]
        private Vector2 _attenuation;

        [SerializeField, MinMaxSlider(MinFrequency, MaxFrequency)]
        private Vector2 _frequency;

        [JsonIgnore]
        public bool Enabled => _enabled;

        [JsonIgnore]
        public float Amplitude => UnityEngine.Random.Range(_amplitude.x, _amplitude.y);

        [JsonIgnore]
        public float Attenuation => UnityEngine.Random.Range(_attenuation.x, _attenuation.y);

        [JsonIgnore]
        public float Frequency => UnityEngine.Random.Range(_frequency.x, _frequency.y);

        public ShakeSettings(bool enabled,
                             Vector2 amplitudeRange,
                             Vector2 attenuationRange,
                             Vector2 frequencyRange)
        {
            _enabled = enabled;
            _amplitude = amplitudeRange;
            _attenuation = attenuationRange;
            _frequency = frequencyRange;
        }
    }
}