using System;

namespace SpaceAce.Gameplay.Shooting
{
    public interface IDamageable
    {
        event EventHandler<DamageReceivedEventArgs> DamageReceived;

        Guid ID { get; }

        void ApplyDamage(float damage);
    }
}