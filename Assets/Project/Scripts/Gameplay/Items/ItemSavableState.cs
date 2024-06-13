using SpaceAce.Main.Factories.SavedItemsFactories;

namespace SpaceAce.Gameplay.Items
{
    public abstract class ItemSavableState
    {
        public Size Size { get; }
        public Quality Quality { get; }
        public float Price { get; }

        public ItemSavableState(Size size, Quality quality, float price)
        {
            Size = size;
            Quality = quality;
            Price = price;
        }

        public abstract IItem Recreate(SavedItemsServices services);
    }
}