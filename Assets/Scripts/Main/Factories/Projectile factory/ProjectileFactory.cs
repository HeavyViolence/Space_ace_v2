using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Players;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.ProjectileFactories
{
    public sealed class ProjectileFactory
    {
        private static readonly int s_playerProjectilesLayerMask = LayerMask.NameToLayer("Player projectiles");
        private static readonly int s_enemyProjectilesLayerMask = LayerMask.NameToLayer("Enemy projectiles");

        private static readonly int s_playerProjectilesSortingLayer = SortingLayer.NameToID("Player projectiles");
        private static readonly int s_enemyProjectilesSortingLayer = SortingLayer.NameToID("Enemy projectiles");

        private readonly Dictionary<ProjectileSkin, GameObject> _projectilePrefabs = new();
        private readonly Dictionary<ProjectileSkin, Stack<ProjectileCache>> _cachedProjectiles = new();
        private readonly Dictionary<ProjectileSkin, Transform> _projectileAnchors = new();
        private readonly GameObject _projectileMasterAnchor = new("Projectiles object pools");

        private readonly DiContainer _diContainer;
        private readonly ProjectileFactoryConfig _config;

        public ProjectileFactory(DiContainer diContainer, ProjectileFactoryConfig config)
        {
            _diContainer = diContainer ?? throw new ArgumentNullException();

            if (config == null) throw new ArgumentNullException();
            _config = config;

            foreach (ProjectileSlot slot in config.ProjectileSlots)
            {
                _projectilePrefabs.Add(slot.Skin, slot.Prefab);

                GameObject anchor = new($"Projectile pool of {slot.Skin}");
                anchor.transform.parent = _projectileMasterAnchor.transform;

                _projectileAnchors.Add(slot.Skin, anchor.transform);
            }
        }

        public ProjectileCache Create(object user, ProjectileSkin skin, Size size)
        {
            ProjectileCache cachedProjectile;
            Vector3 scale = Vector3.one;

            if (_cachedProjectiles.TryGetValue(skin, out Stack<ProjectileCache> stack) == true &&
                stack.TryPop(out ProjectileCache cache) == true)
            {
                cachedProjectile = cache;
            }
            else
            {
                if (_projectilePrefabs.TryGetValue(skin, out GameObject projectilePrefab) == true)
                {
                    GameObject newProjectile = _diContainer.InstantiatePrefab(projectilePrefab);

                    if (newProjectile.TryGetComponent(out SpriteRenderer renderer) == false) throw new MissingComponentException($"{typeof(SpriteRenderer)}");
                    if (newProjectile.TryGetComponent(out DamageDealer damageDealer) == false) throw new MissingComponentException($"{typeof(DamageDealer)}");
                    if (newProjectile.TryGetComponent(out IEscapable escapable) == false) throw new MissingComponentException($"{typeof(IEscapable)}");
                    if (newProjectile.TryGetComponent(out IMovementBehaviourSupplier supplier) == false) throw new MissingComponentException($"{typeof(IMovementBehaviourSupplier)}");

                    cachedProjectile = new(newProjectile, renderer, damageDealer, escapable, supplier);
                }
                else throw new Exception($"Projectile of a requested skin ({skin}) doesn't exist in the config!");
            }

            switch (size)
            {
                case Size.Small:
                    {
                        scale = new(_config.SmallProjectileScale, _config.SmallProjectileScale, _config.SmallProjectileScale);
                        break;
                    }
                case Size.Large:
                    {
                        scale = new(_config.LargeProjectileScale, _config.LargeProjectileScale, _config.LargeProjectileScale);
                        break;
                    }
            }

            cachedProjectile.Object.SetActive(true);
            cachedProjectile.Transform.parent = null;
            cachedProjectile.Transform.localScale = scale;

            if (user is Player)
            {
                cachedProjectile.Object.layer = s_playerProjectilesLayerMask;
                cachedProjectile.SpriteRenderer.sortingLayerID = s_playerProjectilesSortingLayer;
            }
            else
            {
                cachedProjectile.Object.layer = s_enemyProjectilesLayerMask;
                cachedProjectile.SpriteRenderer.sortingLayerID = s_enemyProjectilesSortingLayer;
            }

            return cachedProjectile;
        }

        public void Release(ProjectileSkin skin, ProjectileCache projectile)
        {
            if (projectile is null) throw new ArgumentNullException();

            projectile.Object.SetActive(false);
            projectile.Transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            projectile.Transform.localScale = Vector3.one;
            projectile.Transform.parent = _projectileAnchors[skin];

            if (_cachedProjectiles.TryGetValue(skin, out Stack<ProjectileCache> stack) == true)
            {
                stack.Push(projectile);
            }
            else
            {
                Stack<ProjectileCache> newStack = new();
                newStack.Push(projectile);

                _cachedProjectiles.Add(skin, newStack);
            }
        }
    }
}