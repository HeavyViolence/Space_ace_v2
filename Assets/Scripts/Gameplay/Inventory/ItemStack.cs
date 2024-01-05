using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Inventories
{
    public abstract class ItemStack : IEquatable<ItemStack>
    {
        public event EventHandler AmountChanged, Depleted;

        public IItem Item { get; }
        public int Amount { get; private set; }

        public ItemStack(IItem item, int amount)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item),
                $"Attempted to pass an empty {typeof(IItem)}!");

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount),
                    "Amount must be positive!");

            Amount = amount;
        }

        public void Add(int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount),
                    "Amount must be positive!");

            Amount += amount;
            AmountChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Remove(int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount),
                    "Amount must be positive!");

            Amount = Mathf.Clamp(Amount - amount, 0, int.MaxValue);
            AmountChanged?.Invoke(this, EventArgs.Empty);

            if (Amount == 0) Depleted?.Invoke(this, EventArgs.Empty);
        }

        public bool Sell(int amount, out float sellPrice)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount),
                    "Amount must be positive!");

            if (Item.Tradable == true)
            {
                int amountToSell = Mathf.Clamp(amount, 0, Amount);
                Remove(amountToSell);

                sellPrice = amountToSell * Item.Price;
                return true;
            }

            sellPrice = float.NaN;
            return false;
        }

        public bool Use()
        {
            if (Item.Usable == true && Amount > 0 && Item.Use() == true)
            {
                Remove(1);
                return true;
            }

            return false;
        }

        public abstract ItemStackSavableState GetSavableState();

        public override bool Equals(object obj) => obj is not null && Equals(obj as ItemStack);

        public virtual bool Equals(ItemStack other) => other is not null && Item.Equals(other.Item);

        public override int GetHashCode() => Item.GetHashCode();
    }
}