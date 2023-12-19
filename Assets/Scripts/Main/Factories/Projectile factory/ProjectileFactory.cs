using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class ProjectileFactory
    {
        private readonly Dictionary<ProjectileSkin, GameObject> _projectilePrefabs = new();
        private readonly Dictionary<ProjectileSkin, Stack<CachedProjectile>> _cachedProjectiles = new();

        private readonly LayerMask _playerProjectilesLayerMask;
        private readonly LayerMask _enemyProjectilesLayerMask;

        private readonly string _playerProjectilesSortingLayer;
        private readonly string _enemyProjectilesSortingLayer;

        private readonly DiContainer _diContainer;

        public ProjectileFactory(ProjectileFactoryConfig config, DiContainer diContainer)
        {
            if (config == null) throw new ArgumentNullException(nameof(config),
                $"Attempted to pass an empty {typeof(ProjectileFactoryConfig)}!");

            foreach (var slot in config.Slots) _projectilePrefabs.Add(slot.Skin, slot.Prefab);

            _playerProjectilesLayerMask = config.PlayerProjectilesLayerMask;
            _enemyProjectilesLayerMask = config.EnemyProjectilesLayerMask;

            _playerProjectilesSortingLayer = config.PlayerProjectilesSortingLayer;
            _enemyProjectilesSortingLayer = config.EnemyProjectilesSortingLayer;

            _diContainer = diContainer ?? throw new ArgumentNullException(nameof(diContainer),
                $"Attempted to pass an empty {typeof(DiContainer)}!");
        }

        public CachedProjectile Create(ProjectileRequestor requestor, ProjectileSkin skin)
        {
            if (_cachedProjectiles.TryGetValue(skin, out Stack<CachedProjectile> stack) == true && stack.Count > 0)
            {
                CachedProjectile cachedProjectile = stack.Pop();
                cachedProjectile.Instance.SetActive(true);

                switch (requestor)
                {
                    case ProjectileRequestor.Player:
                        {
                            cachedProjectile.Instance.layer = _playerProjectilesLayerMask;
                            cachedProjectile.SpriteRenderer.sortingLayerName = _playerProjectilesSortingLayer;

                            break;
                        }
                    case ProjectileRequestor.Enemy:
                        {
                            cachedProjectile.Instance.layer = _enemyProjectilesLayerMask;
                            cachedProjectile.SpriteRenderer.sortingLayerName = _enemyProjectilesSortingLayer;

                            break;
                        }
                }

                return cachedProjectile;
            }

            if (_projectilePrefabs.TryGetValue(skin, out GameObject projectilePrefab) == true)
            {
                GameObject newProjectile = _diContainer.InstantiatePrefab(projectilePrefab);

                var renderer = newProjectile.GetComponentInChildren<SpriteRenderer>();

                if (renderer == null)
                    throw new MissingComponentException($"Projectile is missing {typeof(SpriteRenderer)}!");

                var damageDealer = newProjectile.GetComponentInChildren<DamageDealer>();

                if (damageDealer == null)
                    throw new MissingComponentException($"Projectile is missing {typeof(DamageDealer)}!");

                var escapable = newProjectile.GetComponentInChildren<IEscapable>();

                if (escapable == null)
                    throw new MissingComponentException($"Projectile is missing {typeof(IEscapable)}!");

                var behaviourSupplier = newProjectile.GetComponentInChildren<IMovementBehaviourSupplier>();

                if (behaviourSupplier == null)
                    throw new MissingComponentException($"Projectile is missing {typeof(IMovementBehaviourSupplier)}!");

                switch (requestor)
                {
                    case ProjectileRequestor.Player:
                        {
                            newProjectile.layer = _playerProjectilesLayerMask;
                            renderer.sortingLayerName = _playerProjectilesSortingLayer;

                            break;
                        }
                    case ProjectileRequestor.Enemy:
                        {
                            newProjectile.layer = _enemyProjectilesLayerMask;
                            renderer.sortingLayerName = _enemyProjectilesSortingLayer;

                            break;
                        }
                }

                return new(newProjectile, renderer, damageDealer, escapable, behaviourSupplier);
            }

            throw new Exception($"{typeof(ProjectileFactory)} doesn't contain a projectile prefab of {skin}!");
        }

        public void Release(CachedProjectile projectile, ProjectileSkin skin)
        {
            if (projectile is null) throw new ArgumentNullException(nameof(projectile),
                $"Attempted to pass an empty {typeof(CachedProjectile)}!");

            projectile.Instance.SetActive(false);
            projectile.Instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

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