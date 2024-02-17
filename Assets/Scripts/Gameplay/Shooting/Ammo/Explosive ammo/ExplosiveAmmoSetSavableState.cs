using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class ExplosiveAmmoSetSavableState : AmmoSetSavableState
    {
        public float ExplosionRadius { get; }
        public float ExplosionDamage { get; }

        public ExplosiveAmmoSetSavableState(Size size,
                                            Quality quality,
                                            float price,
                                            int amount,
                                            float heatGeneration,
                                            float speed,
                                            float damage,
                                            float explosionRadius,
                                            float explosionDamage) : base(size,
                                                                          quality,
                                                                          price,
                                                                          amount,
                                                                          heatGeneration,
                                                                          speed,
                                                                          damage)
        {
            ExplosionRadius = explosionRadius;
            ExplosionDamage = explosionDamage;
        }
    }
}