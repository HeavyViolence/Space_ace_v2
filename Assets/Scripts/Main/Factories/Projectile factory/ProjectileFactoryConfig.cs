using NaughtyAttributes;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [CreateAssetMenu(fileName = "Projectile factory config",
                     menuName = "Space ace/Configs/Factories/Projectile factory config")]
    public sealed class ProjectileFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<ProjectileSlot> _skins;

        [SerializeField]
        private LayerMask _playerProjectilesLayerMask;

        [SerializeField, SortingLayer]
        private string _playerProjectilesSortingLayer;

        [SerializeField]
        private LayerMask _enemyProjectilesLayerMask;

        [SerializeField, SortingLayer]
        private string _enemyProjectilesSortingLayer;

        public IEnumerable<ProjectileSlot> Slots => _skins;

        public LayerMask PlayerProjectilesLayerMask => _playerProjectilesLayerMask;
        public string PlayerProjectilesSortingLayer => _playerProjectilesSortingLayer;

        public LayerMask EnemyProjectilesLayerMask => _enemyProjectilesLayerMask;
        public string EnemyProjectilesSortingLayer => _enemyProjectilesSortingLayer;
    }
}