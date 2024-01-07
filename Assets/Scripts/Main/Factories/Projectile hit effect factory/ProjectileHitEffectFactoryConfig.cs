using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [CreateAssetMenu(fileName = "Projectile hit effect factory config",
                     menuName = "Space ace/Configs/Factories/Projectile hit effect factory config")]
    public sealed class ProjectileHitEffectFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<ProjectileHitEffectFactoryConfigSlot> _hitEffects;

        public IEnumerable<ProjectileHitEffectFactoryConfigSlot> HitEffects => _hitEffects;
    }
}