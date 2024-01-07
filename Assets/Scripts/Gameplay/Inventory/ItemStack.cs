using Cysharp.Threading.Tasks;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Inventories
{
    public abstract class ItemStack : IEquatable<ItemStack>
    {
        public event EventHandler AmountChanged, Depleted;

        protected readonly IItem Item;

        public int Amount { get; private set; }

        public ItemSize ItemSize => Item.Size;
        public ItemQuality ItemQuality => Item.Quality;

        public float ItemPrice => Item.Price;

        public bool ItemTradable => Item.Tradable;
        public bool ItemUsable => Item.Usable;

        public async UniTask<string> GetItemNameAsync() => await Item.GetNameAsync();
        public async UniTask<string> GetItemDescriptionAsync() => await Item.GetDescriptionAsync();

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

            if (ItemTradable == true)
            {
                int amountToSell = Mathf.Clamp(amount, 0, Amount);
                Remove(amountToSell);

                sellPrice = amountToSell * ItemPrice;
                return true;
            }

            sellPrice = 0f;
            return false;
        }

        public async UniTask<bool> UseAsync(CancellationToken token = default, params object[] args)
        {
            if (Amount > 0) return await Item.UseAsync(this, token, args);
            return false;
        }

        public abstract ItemStackSavableState GetSavableState();

        public override bool Equals(object obj) => obj is not null && Equals(obj as ItemStack);

        public virtual bool Equals(ItemStack other) => other is not null && Item.Equals(other.Item);

        public override int GetHashCode() => Item.GetHashCode();
    }
}