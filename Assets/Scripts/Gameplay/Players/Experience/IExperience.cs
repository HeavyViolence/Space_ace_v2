using SpaceAce.Auxiliary;

using System;

namespace SpaceAce.Gameplay.Players
{
    public interface IExperience
    {
        event EventHandler<FloatValueChangedEventArgs> ValueChanged;

        float Value { get; }

        bool TryEarn(float experienceLoss);
        void Add(float amount);
        void Clear();
    }
}