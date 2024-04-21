using SpaceAce.Auxiliary;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public interface IDurabilityView
    {
        event EventHandler<FloatValueChangedEventArgs> ValueChanged, MaxValueChanged, RegenChanged;

        Sprite Icon { get; }

        float Value { get; }
        float MaxValue { get; }
        float ValueNormalized => Value / MaxValue;
        float ValuePercentage => ValueNormalized * 100f;
        float Regen { get; }
    }
}