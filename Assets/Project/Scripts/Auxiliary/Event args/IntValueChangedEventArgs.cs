using System;

namespace SpaceAce.Auxiliary
{
    public sealed class IntValueChangedEventArgs : EventArgs
    {
        public int OldValue { get; }
        public int NewValue { get; }
        public int Delta => NewValue - OldValue;
        public int AbsoluteDelta => Math.Abs(Delta);

        public IntValueChangedEventArgs(int oldValue, int newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}