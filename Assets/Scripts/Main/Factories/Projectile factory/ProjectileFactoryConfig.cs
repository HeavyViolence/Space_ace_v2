using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [CreateAssetMenu(fileName = "Projectile factory config",
                     menuName = "Space ace/Configs/Factories/Projectile factory config")]
    public sealed class ProjectileFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private string _playerProjectilesLayerName;

        [SerializeField]
        private string _enemyProjectilesLayerName;

        [SerializeField, Space]
        private List<ProjectileSlot> _projectiles;

        public string PlayerProjectilesLayerName => _playerProjectilesLayerName;
        public string EnemyProjectilesLayerName => _enemyProjectilesLayerName;

        public IEnumerable<ProjectileSlot> ProjectileSlots => _projectiles;
    }
}