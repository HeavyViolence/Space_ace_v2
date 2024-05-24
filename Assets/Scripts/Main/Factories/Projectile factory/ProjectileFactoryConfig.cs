using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories.ProjectileFactories
{
    [CreateAssetMenu(fileName = "Projectile factory config",
                     menuName = "Space ace/Configs/Factories/Projectile factory config")]
    public sealed class ProjectileFactoryConfig : ScriptableObject
    {
        #region projectile scale per size

        public const float MinProjectileScale = 0.5f;
        public const float MaxProjectileScale = 2f;

        [SerializeField, Range(MinProjectileScale, MaxProjectileScale)]
        private float _smallProjectileScale = MinProjectileScale;

        public float SmallProjectileScale => _smallProjectileScale;

        [SerializeField, Range(MinProjectileScale, MaxProjectileScale)]
        private float _largeProjectileScale = MinProjectileScale;

        public float LargeProjectileScale => _largeProjectileScale;

        #endregion

        #region projectiles

        [SerializeField, Space]
        private List<ProjectileSlot> _projectiles;

        public IEnumerable<ProjectileSlot> ProjectileSlots => _projectiles;

        #endregion
    }
}