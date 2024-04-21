using SpaceAce.Auxiliary;

using System;

namespace SpaceAce.Gameplay.Damage
{
    public interface IArmorView
    {
        event EventHandler<FloatValueChangedEventArgs> ValueChanged;

        float Value { get; }
    }
}