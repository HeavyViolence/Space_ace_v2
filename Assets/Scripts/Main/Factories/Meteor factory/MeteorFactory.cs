using SpaceAce.Gameplay.Movement;
using SpaceAce.UI;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.MeteorFactories
{
    public sealed class MeteorFactory
    {
        private readonly Dictionary<MeteorType, GameObject> _prefabs;
        private readonly Dictionary<MeteorType, Stack<CachedMeteor>> _objectPools = new();
        private readonly Dictionary<MeteorType, Transform> _objectPoolsAnchors = new();
        private readonly GameObject _masterAnchor = new($"Meteors object pools");
        private readonly DiContainer _container;

        public MeteorFactory(IEnumerable<KeyValuePair<MeteorType, GameObject>> prefabs, DiContainer container)
        {
            if (prefabs is null) throw new ArgumentNullException();
            _prefabs = new(prefabs);

            _container = container ?? throw new ArgumentNullException();

            BuildObjectPoolsAnchors();
        }

        private void BuildObjectPoolsAnchors()
        {
            Array types = Enum.GetValues(typeof(MeteorType));

            foreach (MeteorType type in types)
            {
                GameObject anchor = new($"{type.ToString().ToLower()} meteors");
                anchor.transform.parent = _masterAnchor.transform;

                _objectPoolsAnchors.Add(type, anchor.transform);
            }
        }

        public CachedMeteor Create(MeteorType type, Vector3 position)
        {
            if (_objectPools.TryGetValue(type, out Stack<CachedMeteor> pool) == true &&
                pool.TryPop(out CachedMeteor cache) == true)
            {
                cache.Object.SetActive(true);
                cache.Transform.parent = null;
                cache.Transform.position = position;

                return cache;
            }

            if (_prefabs.TryGetValue(type, out GameObject prefab) == true)
            {
                GameObject meteor = _container.InstantiatePrefab(prefab);

                if (meteor.TryGetComponent(out Transform transform) == false) throw new MissingComponentException($"{typeof(Transform)}");
                if (meteor.TryGetComponent(out IMovementBehaviourSupplier supplier) == false) throw new MissingComponentException($"{typeof(IMovementBehaviourSupplier)}");
                if (meteor.TryGetComponent(out IEntityView view) == false) throw new MissingComponentException($"{typeof(IEntityView)}");

                transform.position = position;

                return new(meteor, transform, supplier, view);
            }

            throw new MissingMemberException($"There is no meteor prefab of type {type} in the config!");
        }

        public void Release(MeteorType type, CachedMeteor meteor)
        {
            meteor.Object.SetActive(false);
            meteor.Transform.parent = _objectPoolsAnchors[type];
            meteor.Transform.position = Vector3.zero;
            meteor.Transform.localScale = Vector3.one;

            if (_objectPools.TryGetValue(type, out Stack<CachedMeteor> pool) == true)
            {
                pool.Push(meteor);
            }
            else
            {
                Stack<CachedMeteor> newPool = new();
                newPool.Push(meteor);

                _objectPools.Add(type, newPool);
            }
        }
    }
}