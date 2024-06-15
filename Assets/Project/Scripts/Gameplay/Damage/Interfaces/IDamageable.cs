using System;

namespace SpaceAce.Gameplay.Damage
{
    public interface IDamageable
    {
        Guid ID { get; }

        void ApplyDamage(float damage, float armorIgnoring = 0f);
    }
}