using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main.Localization;

using System;
using System.Collections.Generic;

namespace SpaceAce.Main.Factories
{
    public sealed class ItemFactory
    {
        private readonly Localizer _localizer;
        private readonly GameStateLoader _loader;

        public ItemFactory(Localizer localizer,
                           GameStateLoader loader)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer),
                $"Attempted to pass an empty {typeof(Localizer)}!");

            _loader = loader ?? throw new ArgumentNullException(nameof(loader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");
        }

        public Item Create(ItemSnapshot snapshot)
        {
            if (snapshot is null)
                throw new ArgumentNullException(nameof(snapshot),
                    $"Attempted to pass an empty {typeof(ItemSnapshot)}!");

            return snapshot.RecreateItem(_localizer, _loader);
        }

        public IEnumerable<Item> BatchCreate(IEnumerable<ItemSnapshot> snapshots)
        {
            if (snapshots is null)
                throw new ArgumentNullException(nameof(snapshots),
                    $"Attempted to pass an empty {typeof(IEnumerable<ItemSnapshot>)}!");

            List<Item> items = new();

            foreach (var snapshot in snapshots)
            {
                Item item = Create(snapshot);
                items.Add(item);
            }

            return items;
        }
    }
}