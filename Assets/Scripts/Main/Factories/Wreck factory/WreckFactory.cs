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
        private readonly Dictionary<WreckType, Stack<CachedWreck>> _objectPools = new();
        private readonly Dictionary<WreckType, Transform> _objectPoolsAnchors = new();
        private readonly GameObject _masterAnchor = new($"Wreck object pools");
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
                GameObject anchor = new($"{type.ToString().ToLower()} wrecks");
                anchor.transform.parent = _masterAnchor.transform;

                _objectPoolsAnchors.Add(type, anchor.transform);
            }
        }

        public CachedWreck Create(WreckType type, Vector3 position)
        {
            if (_objectPools.TryGetValue(type, out Stack<CachedWreck> pool) == true &&
                pool.TryPop(out CachedWreck cache) == true)
            {
                cache.Object.SetActive(true);
                cache.Transform.parent = null;
                cache.Transform.position = position;

                return cache;
            }

            if (_prefabs.TryGetValue(type, out GameObject prefab) == true)
            {
                GameObject wreck = _container.InstantiatePrefab(prefab);

                if (wreck.TryGetComponent(out Transform transform) == false) throw new MissingComponentException($"{typeof(Transform)}");
                if (wreck.TryGetComponent(out DamageDealer damageDealer) == false) throw new MissingComponentException($"{typeof(DamageDealer)}");
                if (wreck.TryGetComponent(out IMovementBehaviourSupplier supplier) == false) throw new MissingComponentException($"{typeof(IMovementBehaviourSupplier)}");
                if (wreck.TryGetComponent(out IEntityView view) == false) throw new MissingComponentException($"{typeof(IEntityView)}");

                transform.position = position;

                return new(wreck, transform, damageDealer, supplier, view);
            }

            throw new MissingMemberException($"There is no wreck prefab of type {type} in the config!");
        }

        public void Release(WreckType type, CachedWreck wreck)
        {
            wreck.Object.SetActive(false);
            wreck.Transform.parent = _objectPoolsAnchors[type];
            wreck.Transform.position = Vector3.zero;
            wreck.Transform.localScale = Vector3.one;

            if (_objectPools.TryGetValue(type, out Stack<CachedWreck> pool) == true)
            {
                pool.Push(wreck);
            }
            else
            {
                Stack<CachedWreck> newPool = new();
                newPool.Push(wreck);

                _objectPools.Add(type, newPool);
            }
        }
    }
}