using SpaceAce.Auxiliary.Easing;

using System;
using System.Collections.Generic;

namespace SpaceAce.Auxiliary.AnimatedValues
{
    public sealed class AnimatedFloat
    {
        private readonly HashSet<AnimatedFloatEntry> _entries = new();
        private readonly HashSet<AnimatedFloatEntry> _entriesToBeDeleted = new();
        private readonly EasingService _easingService;
        private readonly EasingMode _easingMode;
        private readonly float _duration;

        private float _finishedValuesTotal = 0f;

        public bool Locked { get; private set; } = false;

        public AnimatedFloat(EasingService service, EasingMode mode, float duration)
        {
            _easingService = service ?? throw new ArgumentNullException();

            _easingMode = mode;

            if (duration <= 0f) throw new ArgumentOutOfRangeException();
            _duration = duration;
        }

        public bool Add(float value)
        {
            if (Locked == true) return false;

            AnimatedFloatEntry entry = new(_easingService, _easingMode, value, _duration);
            _entries.Add(entry);

            return true;
        }

        public void UnlockAdd(float value)
        {
            Unlock();
            Add(value);
        }

        public float GetValue()
        {
            if (_entries.Count == 0) return _finishedValuesTotal;

            float unfinishedValuesTotal = 0f;

            foreach (var entry in _entries)
                unfinishedValuesTotal += entry.Value;

            return unfinishedValuesTotal + _finishedValuesTotal;
        }

        public void ResetLock()
        {
            Reset();
            Lock();
        }

        public void Reset()
        {
            _entries.Clear();
            _entriesToBeDeleted.Clear();
            _finishedValuesTotal = 0f;
        }

        public void Lock()
        {
            if (Locked == true) return;
            Locked = true;
        }

        public void Unlock()
        {
            if (Locked == false) return;
            Locked = false;
        }

        public void Update()
        {
            if (Locked == true) return;

            UpdateEntries();
            ClearFinishedEntries();
        }

        private void UpdateEntries()
        {
            if (_entries.Count == 0) return;

            foreach (var entry in _entries)
            {
                entry.Update();

                if (entry.AnimationCompleted == true)
                {
                    _entriesToBeDeleted.Add(entry);
                }
            }
        }

        private void ClearFinishedEntries()
        {
            if (_entriesToBeDeleted.Count == 0) return;

            foreach (var entry in _entriesToBeDeleted)
            {
                _finishedValuesTotal += entry.Value;
                _entries.Remove(entry);
            }

            _entriesToBeDeleted.Clear();
        }
    }
}