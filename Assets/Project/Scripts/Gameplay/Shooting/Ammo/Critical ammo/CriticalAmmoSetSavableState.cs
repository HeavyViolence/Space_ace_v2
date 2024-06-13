using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class CriticalAmmoSetSavableState : AmmoSetSavableState
    {
        public float CriticalDamageProbability { get; }
        public float CriticalDamage { get; }

        public CriticalAmmoSetSavableState(Size size,
                                           Quality quality,
                                           float price,
                                           int amount,
                                           float heatGeneration,
                                           float speed,
                                           float damage,
                                           float criticalDamageProbability,
                                           float criticalDamage) : base(size,
                                                                        quality,
                                                                        price,
                                                                        amount,
                                                                        heatGeneration,
                                                                        speed,
                                                                        damage)
        {
            CriticalDamageProbability = criticalDamageProbability;
            CriticalDamage = criticalDamage;
        }
    }
}