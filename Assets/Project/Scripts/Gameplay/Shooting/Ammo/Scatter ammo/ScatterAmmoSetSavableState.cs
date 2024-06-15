using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class ScatterAmmoSetSavableState : AmmoSetSavableState
    {
        public float FireRateIncrease { get; }
        public float DispersionIncrease { get; }

        public ScatterAmmoSetSavableState(Size size,
                                          Quality quality,
                                          float price,
                                          int amount,
                                          float heatGeneration,
                                          float speed,
                                          float damage,
                                          float fireRateIncrease,
                                          float dispersionIncrease) : base(size,
                                                                           quality,
                                                                           price,
                                                                           amount,
                                                                           heatGeneration,
                                                                           speed,
                                                                           damage)
        {
            FireRateIncrease = fireRateIncrease;
            DispersionIncrease = dispersionIncrease;
        }
    }
}