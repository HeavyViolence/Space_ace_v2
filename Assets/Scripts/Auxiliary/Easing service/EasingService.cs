using System;

using UnityEngine;

namespace SpaceAce.Auxiliary
{
    public sealed class EasingService
    {
        private EasingServiceConfig _config;

        public EasingService(EasingServiceConfig config)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;
        }

        public float GetEntranceFastEasingFactor(float time) =>
            _config.EntranceFast.Evaluate(Mathf.Clamp01(time));

        public float GetExitFastEasingFactor(float time) =>
            1f - GetEntranceFastEasingFactor(time);

        public float GetEntranceSlowEasingFactor(float time) =>
            _config.EntranceSlow.Evaluate(Mathf.Clamp01(time));

        public float GetExitSlowEasingFactor(float time) =>
            1f - GetEntranceSlowEasingFactor(time);

        public float GetEntranceSmoothEasingFactor(float time) =>
            _config.EntranceSmooth.Evaluate(Mathf.Clamp01(time));

        public float GetExitSmoothEasingFactor(float time) =>
            1f - GetEntranceSmoothEasingFactor(time);

        public float GetEntranceSharpEasingFactor(float time) =>
            _config.EntranceSharp.Evaluate(Mathf.Clamp01(time));

        public float GetExitSharpEasingFactor(float time) =>
            1f - GetEntranceSharpEasingFactor(time);

        public float GetEntranceBouncyEasingFactor(float time) =>
            _config.EntranceBouncy.Evaluate(Mathf.Clamp01(time));

        public float GetExitBounceEasingFactor(float time) =>
            1f - GetEntranceBouncyEasingFactor(time);
    }
}