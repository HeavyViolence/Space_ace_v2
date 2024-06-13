using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class RegularAmmoSetSavableState : AmmoSetSavableState
    {
        public RegularAmmoSetSavableState(Size size,
                                          Quality quality,
                                          float price,
                                          int amount,
                                          float heatGeneration,
                                          float speed,
                                          float damage) : base(size,
                                                               quality,
                                                               price,
                                                               amount,
                                                               heatGeneration,
                                                               speed,
                                                               damage)
        { }
    }
}