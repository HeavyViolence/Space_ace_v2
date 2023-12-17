using System;
using System.Collections.Generic;

namespace SpaceAce.Gameplay.Inventories
{
    public sealed class Inventory
    {
        public event EventHandler ContentChanged;

        private readonly HashSet<Item> _content = new();

        public bool AddItem(Item item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item),
                    $"Attempted to add an empty {typeof(Item)}!");

            ContentChanged?.Invoke(this, EventArgs.Empty);

            return _content.Add(item);
        }

        public void AddRange(IEnumerable<Item> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items),
                    $"Attempted to pass an empty {typeof(IEnumerable<Item>)}!");

            foreach (Item item in items) _content.Add(item);

            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool RemoveItem(Item item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item),
                    $"Attempted to remove an empty {typeof(Item)}!");

            return _content.Remove(item);
        }

        public void Clear() => _content.Clear();

        public IEnumerable<Item> GetContent() => _content;

        public IEnumerable<ItemSnapshot> GetContentSnapshot()
        {
            List<ItemSnapshot> snapshots = new(_content.Count);

            foreach (var item in _content)
            {
                ItemSnapshot snapshot = item.GetSnapshot();
                snapshots.Add(snapshot);
            }

            return snapshots;
        }
    }
}