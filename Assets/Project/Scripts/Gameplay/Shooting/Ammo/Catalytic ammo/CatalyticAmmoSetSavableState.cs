using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class CatalyticAmmoSetSavableState : AmmoSetSavableState
    {
        public float FirerateFactorPerShot { get; }

        public CatalyticAmmoSetSavableState(Size size,
                                            Quality quality,
                                            float price,
                                            int amount,
                                            float heatGeneration,
                                            float speed,
                                            float damage,
                                            float firerateFactorPerShot) : base(size,
                                                                                quality,
                                                                                price,
                                                                                amount,
                                                                                heatGeneration,
                                                                                speed,
                                                                                damage)
        {
            FirerateFactorPerShot = firerateFactorPerShot;
        }
    }
}