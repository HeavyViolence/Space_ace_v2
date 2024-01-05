using SpaceAce.Main.Factories;

namespace SpaceAce.Gameplay.Inventories
{
    public abstract class ItemStackSavableState
    {
        public ItemSize Size { get; }
        public ItemQuality Quality { get; }
        public int Amount {  get; }

        public ItemStackSavableState(ItemSize size, ItemQuality quality, int amount)
        {
            Size = size;
            Quality = quality;
            Amount = amount;
        }

        public abstract ItemStack Recreate(SavedItemsServices services);
    }
}