using SpaceAce.Auxiliary;

using System;

namespace SpaceAce.Gameplay.Players
{
    public interface IWallet
    {
        event EventHandler<FloatValueChangedEventArgs> BalanceChanged;

        float Balance { get; }

        bool TryBuy(float price);
        void AddCredits(float amount);
        void Clear();
    }
}