using SpaceAce.Gameplay.Items;
using SpaceAce.Main.Factories.AmmoFactories;
using SpaceAce.Main.Localization;

using System;
using System.Collections.Generic;

namespace SpaceAce.Main.Factories.SavedItemsFactories
{
    public sealed class SavedItemsFactory
    {
        private readonly SavedItemsServices _services;

        public SavedItemsFactory(Localizer localizer,
                                 GameStateLoader gameStateLoader,
                                 AmmoFactory ammoFactory)
        {
            _services = new(localizer, gameStateLoader, ammoFactory);
        }

        public IItem Create(ItemSavableState state)
        {
            if (state is null) throw new ArgumentNullException();
            return state.Recreate(_services);
        }

        public IEnumerable<IItem> BatchCreate(IEnumerable<ItemSavableState> states)
        {
            if (states is null) throw new ArgumentNullException();

            List<IItem> items = new();

            foreach (var state in states)
            {
                IItem item = Create(state);
                items.Add(item);
            }

            return items;
        }
    }
}