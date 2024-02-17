using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Players;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class ProjectileFactory
    {
        private readonly Dictionary<ProjectileSkin, GameObject> _projectilePrefabs = new();
        private readonly Dictionary<ProjectileSkin, GameObject> _projectileAnchors = new();
        private readonly GameObject _projectileMasterAnchor = new("Projectiles master anchor");
        private readonly Dictionary<ProjectileSkin, Stack<CachedProjectile>> _cachedProjectiles = new();

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

                _projectileAnchors.Add(slot.Skin, anchor);
            }
        }

        public CachedProjectile Create(object user, ProjectileSkin skin, Size size)
        {
            if (_cachedProjectiles.TryGetValue(skin, out Stack<CachedProjectile> stack) == true && stack.Count > 0)
            {
                CachedProjectile cachedProjectile = stack.Pop();

                cachedProjectile.Instance.transform.parent = null;
                cachedProjectile.Instance.SetActive(true);

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

                cachedProjectile.Instance.transform.localScale = scale;

                if (user is Player)
                {
                    cachedProjectile.Instance.layer = LayerMask.NameToLayer(_config.PlayerProjectilesLayerName);
                    cachedProjectile.SpriteRenderer.sortingLayerID = SortingLayer.NameToID(_config.PlayerProjectilesLayerName);
                }
                else
                {
                    cachedProjectile.Instance.layer = LayerMask.NameToLayer(_config.EnemyProjectilesLayerName);
                    cachedProjectile.SpriteRenderer.sortingLayerID = SortingLayer.NameToID(_config.EnemyProjectilesLayerName);
                }

                return cachedProjectile;
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

                newProjectile.transform.localScale = scale;

                var renderer = newProjectile.GetComponentInChildren<SpriteRenderer>();

                if (renderer == null)
                    throw new MissingComponentException($"Projectile is missing {typeof(SpriteRenderer)}!");

                var damageDealer = newProjectile.GetComponentInChildren<DamageDealer>();

                if (damageDealer == null)
                    throw new MissingComponentException($"Projectile is missing {typeof(DamageDealer)}!");

                var escapable = newProjectile.GetComponentInChildren<IEscapable>();

                if (escapable == null)
                    throw new Exception($"Projectile doesn't implement {typeof(IEscapable)}!");

                var behaviourSupplier = newProjectile.GetComponentInChildren<IMovementBehaviourSupplier>();

                if (behaviourSupplier == null)
                    throw new Exception($"Projectile doesn't implement {typeof(IMovementBehaviourSupplier)}!");

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

                return new(newProjectile, renderer, damageDealer, escapable, behaviourSupplier);
            }

            throw new Exception($"{typeof(ProjectileFactory)} doesn't contain a projectile prefab of {skin}!");
        }

        public void Release(CachedProjectile projectile, ProjectileSkin skin)
        {
            if (projectile is null) throw new ArgumentNullException();

            projectile.Instance.SetActive(false);
            projectile.Instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            projectile.Instance.transform.localScale = Vector3.one;
            projectile.Instance.transform.parent = _projectileAnchors[skin].transform;

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