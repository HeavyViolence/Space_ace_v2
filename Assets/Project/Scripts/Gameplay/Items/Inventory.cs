using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceAce.Gameplay.Items
{
    public sealed class Inventory
    {
        public const int DefaultCapacity = 1000;

        public event EventHandler ContentChanged;

        private readonly HashSet<IItem> _items;

        public int Capacity { get; }
        public int ItemsCount => _items.Count;
        public float FillNormalized => (float)ItemsCount / Capacity;
        public float FillPercentage => FillNormalized * 100f;

        public Inventory(int capacity = DefaultCapacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException();

            Capacity = capacity;
            _items = new(capacity);
        }

        public bool TryAddItem(IItem item)
        {
            if (item is null) throw new ArgumentNullException();
            if (ItemsCount == Capacity) return false;

            _items.Add(item);
            item.Depleted += ItemDepletedEventHandler;

            ContentChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }

        public bool TryAddItems(IEnumerable<IItem> items, out IEnumerable<IItem> overflow)
        {
            if (items is null) throw new ArgumentNullException();

            if (ItemsCount == Capacity)
            {
                overflow = items;
                return false;
            }

            List<IItem> surplus = new();

            foreach (var item in items)
            {
                if (ItemsCount < Capacity)
                {
                    _items.Add(item);
                    item.Depleted += ItemDepletedEventHandler;
                }
                else
                {
                    surplus.Add(item);
                }
            }

            ContentChanged?.Invoke(this, EventArgs.Empty);

            if (surplus.Count == 0)
            {
                overflow = Enumerable.Empty<IItem>();
                return true;
            }

            overflow = surplus;
            return false;
        }

        public bool Contains(IItem item) => _items.Contains(item);

        public bool TryRemoveItem(IItem item)
        {
            if (_items.Remove(item) == true)
            {
                item.Depleted -= ItemDepletedEventHandler;
                ContentChanged?.Invoke(this, EventArgs.Empty);

                return true;
            }

            return false;
        }

        public void Clear()
        {
            foreach (var item in _items)
            {
                item.Depleted -= ItemDepletedEventHandler;
                item.Dispose();
            }

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

        #region event handlers

        private void ItemDepletedEventHandler(object sender, EventArgs e)
        {
            IItem item = sender as IItem;

            _items.Remove(item);
            item.Dispose();
        }

        #endregion
    }
}