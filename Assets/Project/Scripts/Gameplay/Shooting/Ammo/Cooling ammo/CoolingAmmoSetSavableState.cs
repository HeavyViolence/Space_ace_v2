using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class CoolingAmmoSetSavableState : AmmoSetSavableState
    {
        public float HeatGenerationFactorPerShot { get; }

        public CoolingAmmoSetSavableState(Size size,
                                          Quality quality,
                                          float price,
                                          int amount,
                                          float heatGeneration,
                                          float speed,
                                          float damage,
                                          float heatGenerationFactorPerShot) : base(size,
                                                                                    quality,
                                                                                    price,
                                                                                    amount,
                                                                                    heatGeneration,
                                                                                    speed,
                                                                                    damage)
        {
            HeatGenerationFactorPerShot = heatGenerationFactorPerShot;
        }
    }
}