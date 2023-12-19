using SpaceAce.Gameplay.Damage;

using System;

namespace SpaceAce.Gameplay.Shooting.Ammo.Tips
{
    public abstract class Tip
    {
        public readonly Action<DamageReceiver> DamageBehaviour;

        public Tip(Action<DamageReceiver> damageBehaviour)
        {
            DamageBehaviour = damageBehaviour ?? throw new ArgumentNullException(nameof(damageBehaviour),
                "Attempted to pass an empty damage behaviour!");
        }
    }
}