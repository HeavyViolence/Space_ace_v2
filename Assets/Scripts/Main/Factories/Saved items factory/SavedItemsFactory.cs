using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main.Localization;

using System;
using System.Collections.Generic;

namespace SpaceAce.Main.Factories
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

        public ItemStack Recreate(ItemStackSavableState state)
        {
            if (state is null) throw new ArgumentNullException();
            return state.Recreate(_services);
        }

        public IEnumerable<ItemStack> BatchRecreate(IEnumerable<ItemStackSavableState> states)
        {
            if (states is null) throw new ArgumentNullException();

            List<ItemStack> groups = new();

            foreach (var state in states)
            {
                ItemStack group = Recreate(state);
                groups.Add(group);
            }

            return groups;
        }
    }
}