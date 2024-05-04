using SpaceAce.Gameplay.Items;
using SpaceAce.Main.Factories.SavedItemsFactories;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public abstract class AmmoSetSavableState : ItemSavableState
    {
        public int Amount { get; }
        public float HeatGeneration { get; }
        public float Speed { get; }
        public float Damage { get; }

        public AmmoSetSavableState(Size size,
                                   Quality quality,
                                   float price,
                                   int amount,
                                   float heatGeneration,
                                   float speed,
                                   float damage) : base(size, quality, price)
        {
            Amount = amount;
            HeatGeneration = heatGeneration;
            Speed = speed;
            Damage = damage;
        }

        public sealed override IItem Recreate(SavedItemsServices services) => services.AmmoFactory.Create(this);
    }
}