using SpaceAce.Main.Factories.AmmoFactories;
using SpaceAce.Main.Localization;

using System;

namespace SpaceAce.Main.Factories.SavedItemsFactories
{
    public readonly struct SavedItemsServices
    {
        public Localizer Localizer { get; }
        public GameStateLoader GameStateLoader { get; }
        public AmmoFactory AmmoFactory { get; }

        public SavedItemsServices(Localizer localizer,
                                  GameStateLoader gameStateLoader,
                                  AmmoFactory ammoFactory)
        {
            Localizer = localizer ?? throw new ArgumentNullException();
            GameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            AmmoFactory = ammoFactory ?? throw new ArgumentNullException();
        }
    }
}