using SpaceAce.Auxiliary;

using System;

namespace SpaceAce.Gameplay.Players
{
    public sealed class Experience
    {
        public event EventHandler<FloatValueChangedEventArgs> ValueChanged;

        public float Value { get; private set; }

        public Experience(float value = 0f)
        {
            if (value < 0f) throw new ArgumentOutOfRangeException();
            Value = value;
        }

        public bool TryEarn(float experienceLoss)
        {
            if (experienceLoss > Value) return false;

            float oldValue = Value;
            float newValue = Value - experienceLoss;

            Value -= experienceLoss;
            ValueChanged?.Invoke(this, new(oldValue, newValue));

            return true;
        }

        public void Add(float amount)
        {
            if (amount < 0f) throw new ArgumentOutOfRangeException();
            if (amount == 0f) return;

            float oldValue = Value;
            float newValue = Value + amount;

            Value += amount;
            ValueChanged?.Invoke(this, new(oldValue, newValue));
        }

        public void Clear()
        {
            if (Value == 0f) return;

            float oldValue = Value;
            float newValue = 0f;

            Value = 0f;
            ValueChanged?.Invoke(this, new(oldValue, newValue));
        }

        public void ClearAndAdd(float amount)
        {
            if (amount < 0f) throw new ArgumentOutOfRangeException();

            float oldValue = Value;
            float newValue = amount;

            Value = amount;
            ValueChanged?.Invoke(this, new(oldValue, newValue));
        }
    }
}