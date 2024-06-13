using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.Main.Factories.PlayerShipFactories;
using SpaceAce.UI;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.EnemyShipFactories
{
    public sealed class EnemyShipFactory
    {
        private readonly DiContainer _diContainer;
        private readonly Transform MasterAnchor = new GameObject("Enemy ships").transform;
        private readonly Dictionary<EnemyShipType, GameObject> _prefabs;
        private readonly Dictionary<EnemyShipType, Transform> _anchors;
        private readonly Dictionary<EnemyShipType, Stack<ShipCache>> _objectPools;

        public EnemyShipFactory(DiContainer container, IEnumerable<KeyValuePair<EnemyShipType, GameObject>> prefabs)
        {
            _diContainer = container ?? throw new ArgumentNullException();

            if (prefabs is null) throw new ArgumentNullException();
            _prefabs = new(prefabs);

            _anchors = new(CreateAnchors());
        }

        private IEnumerable<KeyValuePair<EnemyShipType, Transform>> CreateAnchors()
        {
            IEnumerable<EnemyShipType> enemyShipTypesTotal = Enum.GetValues(typeof(EnemyShipType)).Cast<EnemyShipType>();
            Dictionary<EnemyShipType, Transform> anchors = new();

            foreach (EnemyShipType type in enemyShipTypesTotal)
            {
                Transform anchor = new GameObject($"{type}").transform;
                anchor.parent = MasterAnchor;
                anchors.Add(type, anchor);
            }

            return anchors;
        }

        public ShipCache Create(EnemyShipType type, Vector3 position, Quaternion rotation)
        {
            if (_objectPools.TryGetValue(type, out Stack<ShipCache> pool) == true &&
                pool.TryPop(out ShipCache cache) == true)
            {
                cache.Transform.parent = null;
                cache.Transform.position = position;
                cache.Transform.rotation = rotation;
                cache.Transform.gameObject.SetActive(true);

                return cache;
            }

            if (_prefabs.TryGetValue(type, out GameObject prefab) == true)
            {
                GameObject enemyShip = _diContainer.InstantiatePrefab(prefab, position, rotation, null);

                if (enemyShip.TryGetComponent(out Durability durability) == false) throw new MissingComponentException(nameof(Durability));
                if (enemyShip.TryGetComponent(out Armor armor) == false) throw new MissingComponentException(nameof(Armor));
                if (enemyShip.TryGetComponent(out Shooting shooting) == false) throw new MissingComponentException(nameof(Shooting));
                if (enemyShip.TryGetComponent(out IMovementController controller) == false) throw new MissingComponentException(nameof(IMovementController));
                if (enemyShip.TryGetComponent(out IDamageable damageable) == false) throw new MissingComponentException(nameof(IDamageable));
                if (enemyShip.TryGetComponent(out IEntityView view) == false) throw new MissingComponentException(nameof(IEntityView));

                return new(enemyShip, durability, armor, shooting, controller, damageable, view);
            }

            throw new Exception($"There is no enemy ship prefab of type {type} in the config!");
        }

        public void Release(EnemyShipType type, ShipCache cache)
        {
            if (cache is null) throw new ArgumentNullException();

            cache.Transform.parent = _anchors[type];
            cache.Transform.position = Vector3.zero;
            cache.Transform.rotation = Quaternion.identity;
            cache.Transform.gameObject.SetActive(false);

            if (_objectPools.TryGetValue(type, out Stack<ShipCache> pool) == true)
            {
                pool.Push(cache);
            }
            else
            {
                Stack<ShipCache> newPool = new();
                newPool.Push(cache);

                _objectPools.Add(type, newPool);
            }
        }
    }
}