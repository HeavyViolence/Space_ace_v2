using System;

using UnityEngine;

namespace SpaceAce.Auxiliary.Easing
{
    public sealed class EasingService
    {
        private readonly EasingServiceConfig _config;

        public EasingService(EasingServiceConfig config)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;
        }

        public float GetEasedTime(EasingMode mode, float time)
        {
            float timeNormalized = Mathf.Clamp01(time);

            return mode switch
            {
                EasingMode.Fast => _config.Fast.Evaluate(timeNormalized),
                EasingMode.Slow => _config.Slow.Evaluate(timeNormalized),
                EasingMode.Smooth => _config.Smooth.Evaluate(timeNormalized),
                EasingMode.Sharp => _config.Sharp.Evaluate(timeNormalized),
                EasingMode.Wavy => _config.Wavy.Evaluate(timeNormalized),
                _ => 1f,
            };
        }
    }
}