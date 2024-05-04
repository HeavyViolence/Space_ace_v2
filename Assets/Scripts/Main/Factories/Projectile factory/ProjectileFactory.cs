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
        private readonly Dictionary<ProjectileSkin, GameObject> _projectilePrefabs = new();
        private readonly Dictionary<ProjectileSkin, Stack<CachedProjectile>> _cachedProjectiles = new();
        private readonly Dictionary<ProjectileSkin, GameObject> _projectileAnchors = new();
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

                GameObject anchor = new($"Projectile pool of {slot.Skin.ToString().ToLower()}");
                anchor.transform.parent = _projectileMasterAnchor.transform;

                _projectileAnchors.Add(slot.Skin, anchor);
            }
        }

        public CachedProjectile Create(object user, ProjectileSkin skin, Size size)
        {
            if (_cachedProjectiles.TryGetValue(skin, out Stack<CachedProjectile> stack) == true &&
                stack.TryPop(out CachedProjectile cache) == true)
            {
                cache.Object.SetActive(true);
                cache.Transform.parent = null;

                Vector3 scale = Vector3.one;

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

                cache.Transform.localScale = scale;

                if (user is Player)
                {
                    cache.Object.layer = LayerMask.NameToLayer(_config.PlayerProjectilesLayerName);
                    cache.SpriteRenderer.sortingLayerID = SortingLayer.NameToID(_config.PlayerProjectilesLayerName);
                }
                else
                {
                    cache.Object.layer = LayerMask.NameToLayer(_config.EnemyProjectilesLayerName);
                    cache.SpriteRenderer.sortingLayerID = SortingLayer.NameToID(_config.EnemyProjectilesLayerName);
                }

                return cache;
            }

            if (_projectilePrefabs.TryGetValue(skin, out GameObject projectilePrefab) == true)
            {
                GameObject newProjectile = _diContainer.InstantiatePrefab(projectilePrefab);

                Vector3 scale = Vector3.one;

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

                if (newProjectile.TryGetComponent(out SpriteRenderer renderer) == false) throw new MissingComponentException($"{typeof(SpriteRenderer)}");
                if (newProjectile.TryGetComponent(out Transform transform) == false) throw new MissingComponentException($"{typeof(Transform)}");
                if (newProjectile.TryGetComponent(out DamageDealer damageDealer) == false) throw new MissingComponentException($"{typeof(DamageDealer)}");
                if (newProjectile.TryGetComponent(out IEscapable escapable) == false) throw new MissingComponentException($"{typeof(IEscapable)}");
                if (newProjectile.TryGetComponent(out IMovementBehaviourSupplier supplier) == false) throw new MissingComponentException($"{typeof(IMovementBehaviourSupplier)}");

                transform.localScale = scale;

                if (user is Player)
                {
                    newProjectile.layer = LayerMask.NameToLayer(_config.PlayerProjectilesLayerName);
                    renderer.sortingLayerID = SortingLayer.NameToID(_config.PlayerProjectilesLayerName);
                }
                else
                {
                    newProjectile.layer = LayerMask.NameToLayer(_config.EnemyProjectilesLayerName);
                    renderer.sortingLayerID = SortingLayer.NameToID(_config.EnemyProjectilesLayerName);
                }

                return new(newProjectile, transform, renderer, damageDealer, escapable, supplier);
            }

            throw new Exception($"Projectile of a requested skin ({skin}) doesn't exist in the config!");
        }

        public void Release(CachedProjectile projectile, ProjectileSkin skin)
        {
            if (projectile.Incomplete == true) throw new ArgumentNullException();

            projectile.Object.SetActive(false);
            projectile.Transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            projectile.Transform.localScale = Vector3.one;
            projectile.Transform.parent = _projectileAnchors[skin].transform;

            if (_cachedProjectiles.TryGetValue(skin, out Stack<CachedProjectile> stack) == true)
            {
                stack.Push(projectile);
            }
            else
            {
                Stack<CachedProjectile> newStack = new();
                newStack.Push(projectile);

                _cachedProjectiles.Add(skin, newStack);
            }
        }
    }
}