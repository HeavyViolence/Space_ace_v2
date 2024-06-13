using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class NaniteAmmoSetSavableState : AmmoSetSavableState
    {
        public float DamagePerSecond { get; }
        public float DamageDuration { get; }

        public NaniteAmmoSetSavableState(Size size,
                                         Quality quality,
                                         float price,
                                         int amount,
                                         float heatGeneration,
                                         float speed,
                                         float damage,
                                         float damagePerSecond,
                                         float damageDuration) : base(size,
                                                                      quality,
                                                                      price,
                                                                      amount,
                                                                      heatGeneration,
                                                                      speed,
                                                                      damage)
        {
            DamagePerSecond = damagePerSecond;
            DamageDuration = damageDuration;
        }
    }
}