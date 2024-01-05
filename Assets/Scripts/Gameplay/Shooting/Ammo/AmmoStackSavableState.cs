using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main.Factories;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class AmmoStackSavableState : ItemStackSavableState
    {
        public AmmoType Type { get; }
        public ProjectileSkin Skin { get; }

        public AmmoStackSavableState(ItemSize size,
                                     ItemQuality quality,
                                     int amount,
                                     AmmoType type,
                                     ProjectileSkin skin) : base(size, quality, amount)
        {
            Type = type;
            Skin = skin;
        }

        public override ItemStack Recreate(SavedItemsServices services) =>
            services.AmmoFactory.Create(Type, Size, Quality, Skin, Amount);
    }
}