using SpaceAce.Auxiliary;

using System;
using System.Collections.Generic;

namespace SpaceAce.Gameplay.Items
{
    public sealed class Inventory
    {
        public const int DefaultCapacity = 1000;

        public event EventHandler ContentChanged;

        private readonly List<IItem> _items;

        public int ItemsCount => _items.Count;
        public int Capacity => _items.Capacity;

        public Inventory(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException();
            _items = new(Capacity);
        }

        public Inventory()
        {
            _items = new(DefaultCapacity);
        }

        public bool TryAddItem(IItem item)
        {
            if (item is null) throw new ArgumentNullException();
            if (ItemsCount == Capacity) return false;

            _items.Add(item);
            item.Depleted += (sender, args) => _items.Remove(item);

            ContentChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }

        public bool TryAddItems(IEnumerable<IItem> items, out IEnumerable<IItem> leftover)
        {
            if (items is null) throw new ArgumentNullException();

            if (ItemsCount == Capacity)
            {
                leftover = null;
                return false;
            }

            List<IItem> surplus = new();

            foreach (var item in items)
            {
                if (ItemsCount < Capacity)
                {
                    _items.Add(item);
                    item.Depleted += (sender, args) => _items.Remove(item);
                }
                else
                {
                    surplus.Add(item);
                }
            }

            ContentChanged?.Invoke(this, EventArgs.Empty);

            if (surplus.Count == 0)
            {
                leftover = null;
                return true;
            }

            leftover = surplus;
            return false;
        }

        public bool Contains(IItem item) => _items.Contains(item);

        public bool TryRemoveItem(int itemIndex, out IItem removedItem)
        {
            if (AuxMath.ValueInRange(0, ItemsCount - 1, itemIndex) == false)
            {
                removedItem = null;
                return false;
            }

            removedItem = _items[itemIndex];
            _items.RemoveAt(itemIndex);
            ContentChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }

        public void Clear()
        {
            foreach (var item in _items)
                item.Depleted -= (sender, args) => _items.Remove(item);

            _items.Clear();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerable<IItem> GetItems() => _items;

        public IEnumerable<ItemSavableState> GetItemsSavableStates()
        {
            List<ItemSavableState> states = new(_items.Count);

            foreach (var item in _items)
            {
                ItemSavableState state = item.GetSavableState();
                states.Add(state);
            }

            return states;
        }
    }
}