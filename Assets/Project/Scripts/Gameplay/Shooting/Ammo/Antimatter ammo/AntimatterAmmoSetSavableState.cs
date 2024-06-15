using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class AntimatterAmmoSetSavableState : AmmoSetSavableState
    {
        public float DamageFactorPerShot { get; }
        public float FireRateFactorPerShot { get; }

        public AntimatterAmmoSetSavableState(Size size,
                                             Quality quality,
                                             float price,
                                             int amount,
                                             float heatGeneration,
                                             float speed,
                                             float damage,
                                             float damageFactorPerShot,
                                             float fireRateFactorPerShot) : base(size,
                                                                                 quality,
                                                                                 price,
                                                                                 amount,
                                                                                 heatGeneration,
                                                                                 speed,
                                                                                 damage)
        {
            DamageFactorPerShot = damageFactorPerShot;
            FireRateFactorPerShot = fireRateFactorPerShot;
        }
    }
}