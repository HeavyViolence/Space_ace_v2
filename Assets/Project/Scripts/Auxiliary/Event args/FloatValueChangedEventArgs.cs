using System;

namespace SpaceAce.Auxiliary
{
    public sealed class FloatValueChangedEventArgs : EventArgs
    {
        public float OldValue { get; }
        public float NewValue { get; }
        public float Delta => NewValue - OldValue;
        public float AbsoluteDelta => Math.Abs(Delta);

        public FloatValueChangedEventArgs(float oldValue, float newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}