using SpaceAce.Main.Localization;

using System;

namespace SpaceAce.Main.Factories
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
            Localizer = localizer ??
                throw new ArgumentNullException(nameof(localizer),
                $"Attempted to pass an empty {typeof(Localizer)}!");

            GameStateLoader = gameStateLoader ??
                throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempetd to pass an empty {typeof(GameStateLoader)}!");

            AmmoFactory = ammoFactory ??
                throw new ArgumentNullException(nameof(ammoFactory),
                $"Attempted to pass an empty {typeof(AmmoFactory)}!");
        }
    }
}