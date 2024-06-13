using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class EntangledAmmoSetSavableState : AmmoSetSavableState
    {
        public int AmmoLossOnMiss { get; }
        public int AmmoGainOnHit { get; }

        public EntangledAmmoSetSavableState(Size size,
                                            Quality quality,
                                            float price,
                                            int amount,
                                            float heatGeneration,
                                            float speed, 
                                            float damage,
                                            int ammoLossOnMiss,
                                            int ammoGainOnHit) : base(size,
                                                                      quality,
                                                                      price,
                                                                      amount,
                                                                      heatGeneration,
                                                                      speed,
                                                                      damage)
        {
            AmmoLossOnMiss = ammoLossOnMiss;
            AmmoGainOnHit = ammoGainOnHit;
        }
    }
}