using SpaceAce.Gameplay.Items;
using SpaceAce.Main.Factories;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class RegularAmmoSetSavableState : AmmoSetSavableState
    {
        public RegularAmmoSetSavableState(AmmoType type,
                                          Size size,
                                          Quality quality,
                                          float price,
                                          int amount,
                                          float heatGeneration,
                                          float speed,
                                          float damage) : base(type,
                                                               size,
                                                               quality,
                                                               price,
                                                               amount,
                                                               heatGeneration,
                                                               speed,
                                                               damage)
        {
        }

        public override IItem Recreate(SavedItemsServices services) => services.AmmoFactory.Create(this);
    }
}