using SpaceAce.Gameplay.Items;
using SpaceAce.Main.Factories;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class StrangeAmmoSetSavableState : AmmoSetSavableState
    {
        public float AmmoLossProbability { get; }

        public StrangeAmmoSetSavableState(AmmoType type,
                                          Size size,
                                          Quality quality,
                                          float price,
                                          int amount,
                                          float heatGeneration,
                                          float speed,
                                          float damage,
                                          float ammoLossProbability) : base(type,
                                                                            size,
                                                                            quality,
                                                                            price,
                                                                            amount,
                                                                            heatGeneration,
                                                                            speed,
                                                                            damage)
        {
            AmmoLossProbability = ammoLossProbability;
        }

        public override IItem Recreate(SavedItemsServices services) => services.AmmoFactory.Create(this);
    }
}