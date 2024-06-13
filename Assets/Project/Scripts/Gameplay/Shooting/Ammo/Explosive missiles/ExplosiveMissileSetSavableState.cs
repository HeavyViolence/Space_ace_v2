using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class ExplosiveMissileSetSavableState : MissileSetSavableState
    {
        public float ExplosionRadius { get; }
        public float ExplosionDamage { get; }

        public ExplosiveMissileSetSavableState(Size size,
                                               Quality quality,
                                               float price,
                                               int amount,
                                               float heatGeneration,
                                               float speed,
                                               float damage,
                                               float homingSpeed,
                                               float targetingWidth,
                                               float speedGainDuration,
                                               float explosionRadius,
                                               float explosionDamage) : base(size,
                                                                             quality,
                                                                             price,
                                                                             amount,
                                                                             heatGeneration,
                                                                             speed,
                                                                             damage,
                                                                             homingSpeed,
                                                                             targetingWidth,
                                                                             speedGainDuration)
        {
            ExplosionRadius = explosionRadius;
            ExplosionDamage = explosionDamage;
        }
    }
}