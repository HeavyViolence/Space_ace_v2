using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class StabilizingAmmoSetSavableState : AmmoSetSavableState
    {
        public float DispersionFactorPerShot { get; }

        public StabilizingAmmoSetSavableState(Size size,
                                              Quality quality,
                                              float price,
                                              int amount,
                                              float heatGeneration,
                                              float speed,
                                              float damage,
                                              float dispersionFactorPerShot) : base(size,
                                                                                    quality,
                                                                                    price,
                                                                                    amount,
                                                                                    heatGeneration,
                                                                                    speed,
                                                                                    damage)
        {
            DispersionFactorPerShot = dispersionFactorPerShot;
        }
    }
}