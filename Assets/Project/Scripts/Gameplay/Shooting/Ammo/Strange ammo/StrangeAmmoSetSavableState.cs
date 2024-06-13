using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class StrangeAmmoSetSavableState : AmmoSetSavableState
    {
        public float AmmoLossProbability { get; }

        public StrangeAmmoSetSavableState(Size size,
                                          Quality quality,
                                          float price,
                                          int amount,
                                          float heatGeneration,
                                          float speed,
                                          float damage,
                                          float ammoLossProbability) : base(size,
                                                                            quality,
                                                                            price,
                                                                            amount,
                                                                            heatGeneration,
                                                                            speed,
                                                                            damage)
        {
            AmmoLossProbability = ammoLossProbability;
        }
    }
}