using SpaceAce.Gameplay.Damage;
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
        private readonly Dictionary<ProjectileSkin, Stack<CachedProjectile>> _cachedProjectiles = new();

        private readonly DiContainer _diContainer;

        private readonly string _playerProjectilesLayerName;
        private readonly string _enemyProjectilesLayerName;

        public ProjectileFactory(DiContainer diContainer, ProjectileFactoryConfig config)
        {
            _diContainer = diContainer ?? throw new ArgumentNullException();

            if (config == null) throw new ArgumentNullException();

            _playerProjectilesLayerName = config.PlayerProjectilesLayerName;
            _enemyProjectilesLayerName = config.EnemyProjectilesLayerName;

            foreach (ProjectileSlot slot in config.ProjectileSlots)
                _projectilePrefabs.Add(slot.Skin, slot.Prefab);
        }

        public CachedProjectile Create(object user, ProjectileSkin skin)
        {
            if (_cachedProjectiles.TryGetValue(skin, out Stack<CachedProjectile> stack) == true && stack.Count > 0)
            {
                CachedProjectile cachedProjectile = stack.Pop();
                cachedProjectile.Instance.SetActive(true);

                if (user is Player)
                {
                    cachedProjectile.Instance.layer = LayerMask.NameToLayer(_playerProjectilesLayerName);
                    cachedProjectile.SpriteRenderer.sortingLayerID = SortingLayer.NameToID(_playerProjectilesLayerName);
                }
                else
                {
                    cachedProjectile.Instance.layer = LayerMask.NameToLayer(_enemyProjectilesLayerName);
                    cachedProjectile.SpriteRenderer.sortingLayerID = SortingLayer.NameToID(_enemyProjectilesLayerName);
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

                if (user is Player)
                {
                    newProjectile.layer = LayerMask.NameToLayer(_playerProjectilesLayerName);
                    renderer.sortingLayerID = SortingLayer.NameToID(_playerProjectilesLayerName);
                }
                else
                {
                    newProjectile.layer = LayerMask.NameToLayer(_enemyProjectilesLayerName);
                    renderer.sortingLayerID = SortingLayer.NameToID(_enemyProjectilesLayerName);
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