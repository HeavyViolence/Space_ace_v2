using SpaceAce.Auxiliary;

using System;

namespace SpaceAce.Gameplay.Players
{
    public sealed class Wallet
    {
        public event EventHandler<FloatValueChangedEventArgs> BalanceChanged;

        public float Balance { get; private set; }

        public Wallet(float balance = 0f)
        {
            if (balance < 0f) throw new ArgumentOutOfRangeException();
            Balance = balance;
        }

        public bool TryBuy(float price)
        {
            if (price > Balance) return false;

            float oldBalance = Balance;
            float newBalance = Balance - price;

            Balance -= price;
            BalanceChanged?.Invoke(this, new(oldBalance, newBalance));

            return true;
        }

        public void AddCredits(float amount)
        {
            if (amount < 0f) throw new ArgumentOutOfRangeException();
            if (amount == 0f) return;

            float oldBalance = Balance;
            float newBalance = Balance + amount;

            Balance += amount;
            BalanceChanged?.Invoke(this, new(oldBalance, newBalance));
        }

        public void Clear()
        {
            if (Balance == 0f) return;

            float oldBalance = Balance;
            float newBalance = 0f;

            Balance = 0f;
            BalanceChanged?.Invoke(this, new(oldBalance, newBalance));
        }

        public void ClearAndAddCredits(float amount)
        {
            if (amount < 0f) throw new ArgumentOutOfRangeException();

            float oldBalance = Balance;
            float newBalance = amount;

            Balance = amount;
            BalanceChanged?.Invoke(this, new(oldBalance, newBalance));
        }
    }
}