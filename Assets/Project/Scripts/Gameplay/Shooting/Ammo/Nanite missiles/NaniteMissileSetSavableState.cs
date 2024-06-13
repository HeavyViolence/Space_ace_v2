using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class NaniteMissileSetSavableState : MissileSetSavableState
    {
        public float ExplosionRadius { get; }
        public float DamagePerSecond { get; }
        public float DamageDuration { get; }

        public NaniteMissileSetSavableState(Size size,
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
                                            float damagePerSecond,
                                            float damageDuration) : base(size,
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
            DamagePerSecond = damagePerSecond;
            DamageDuration = damageDuration;
        }
    }
}