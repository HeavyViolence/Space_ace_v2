using SpaceAce.Auxiliary;

using System;

namespace SpaceAce.Gameplay.Damage
{
    public interface IDurabilityView
    {
        event EventHandler<FloatValueChangedEventArgs> ValueChanged, MaxValueChanged, RegenChanged;

        float Value { get; }
        float MaxValue { get; }
        float ValueNormalized => Value / MaxValue;
        float ValuePercentage => ValueNormalized * 100f;
        float Regen { get; }
    }
}