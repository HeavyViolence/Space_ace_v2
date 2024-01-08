using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main.Factories;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class AmmoStackSavableState : ItemStackSavableState
    {
        public AmmoType Type { get; }

        public AmmoStackSavableState(AmmoType type,
                                     ItemSize size,
                                     ItemQuality quality,
                                     int amount) : base(size, quality, amount)
        {
            Type = type;
        }

        public override ItemStack Recreate(SavedItemsServices services) =>
            services.AmmoFactory.Create(Type, Size, Quality, Amount);
    }
}