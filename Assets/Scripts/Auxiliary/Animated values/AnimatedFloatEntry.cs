using SpaceAce.Auxiliary.Easing;

using System;

using UnityEngine;

namespace SpaceAce.Auxiliary.AnimatedValues
{
    public sealed class AnimatedFloatEntry : IEquatable<AnimatedFloatEntry>
    {
        private readonly EasingService _easingService;
        private readonly EasingMode _easingMode;
        private readonly float _value;
        private readonly float _duration;

        private float _timer = 0f;

        public Guid ID { get; private set; } = Guid.NewGuid();
        public float Value { get; private set; }
        public bool AnimationCompleted => _timer >= _duration;

        public AnimatedFloatEntry(EasingService service, EasingMode mode, float value, float duration)
        {
            _easingService = service;
            _easingMode = mode;
            _value = value;
            _duration = duration;
        }

        public void Update()
        {
            _timer += Time.deltaTime;

            float normalizedEasedTime = _easingService.GetEasedTime(_easingMode, _timer / _duration);
            Value = Mathf.Lerp(0f, _value, normalizedEasedTime);
        }

        #region interfaces

        public override bool Equals(object obj) => obj is not null && Equals(obj as AnimatedFloatEntry);

        public bool Equals(AnimatedFloatEntry other) => ID == other.ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion
    }
}