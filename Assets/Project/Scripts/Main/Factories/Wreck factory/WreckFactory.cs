using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;
using SpaceAce.UI;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.WreckFactories
{
    public sealed class WreckFactory
    {
        private readonly Dictionary<WreckType, GameObject> _prefabs;
        private readonly Dictionary<WreckType, Stack<WreckCache>> _objectPools = new();
        private readonly Dictionary<WreckType, Transform> _objectPoolsAnchors = new();
        private readonly Transform _masterAnchor = new GameObject("Wreck object pools").transform;
        private readonly DiContainer _container;

        public WreckFactory(IEnumerable<KeyValuePair<WreckType, GameObject>> prefabs, DiContainer container)
        {
            if (prefabs is null) throw new ArgumentNullException();
            _prefabs = new(prefabs);

            _container = container ?? throw new ArgumentNullException();

            BuildObjectPoolsAnchors();
        }

        private void BuildObjectPoolsAnchors()
        {
            Array types = Enum.GetValues(typeof(WreckType));

            foreach (WreckType type in types)
            {
                Transform anchor = new GameObject($"{type}").transform;
                anchor.parent = _masterAnchor;

                _objectPoolsAnchors.Add(type, anchor);
            }
        }

        public WreckCache Create(WreckType type, Vector3 position, Quaternion rotation)
        {
            if (_objectPools.TryGetValue(type, out Stack<WreckCache> pool) == true &&
                pool.TryPop(out WreckCache cache) == true)
            {
                cache.Object.SetActive(true);
                cache.Transform.SetPositionAndRotation(position, rotation);
                cache.Transform.parent = null;

                return cache;
            }

            if (_prefabs.TryGetValue(type, out GameObject prefab) == true)
            {
                GameObject wreck = _container.InstantiatePrefab(prefab, position, rotation, null);

                if (wreck.TryGetComponent(out DamageDealer damageDealer) == false) throw new MissingComponentException($"{typeof(DamageDealer)}");
                if (wreck.TryGetComponent(out IMovementBehaviourSupplier supplier) == false) throw new MissingComponentException($"{typeof(IMovementBehaviourSupplier)}");
                if (wreck.TryGetComponent(out IEntityView view) == false) throw new MissingComponentException($"{typeof(IEntityView)}");

                return new(wreck, damageDealer, supplier, view);
            }

            throw new Exception($"There is no wreck prefab of type {type} in the config!");
        }

        public void Release(WreckType type, WreckCache wreck)
        {
            if (wreck is null) throw new ArgumentNullException();

            wreck.Object.SetActive(false);
            wreck.Transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            wreck.Transform.parent = _objectPoolsAnchors[type];
            wreck.Transform.localScale = Vector3.one;

            if (_objectPools.TryGetValue(type, out Stack<WreckCache> pool) == true)
            {
                pool.Push(wreck);
            }
            else
            {
                Stack<WreckCache> newPool = new();
                newPool.Push(wreck);

                _objectPools.Add(type, newPool);
            }
        }
    }
}