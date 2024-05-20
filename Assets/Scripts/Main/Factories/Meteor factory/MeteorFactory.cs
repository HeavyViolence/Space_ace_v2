using SpaceAce.Gameplay.Damage;
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
        private readonly Dictionary<MeteorType, Stack<MeteorCache>> _objectPools = new();
        private readonly Dictionary<MeteorType, Transform> _objectPoolsAnchors = new();
        private readonly GameObject _masterAnchor = new($"Meteor object pools");
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

        public MeteorCache Create(MeteorType type, Vector3 position, Quaternion rotation)
        {
            if (_objectPools.TryGetValue(type, out Stack<MeteorCache> pool) == true &&
                pool.TryPop(out MeteorCache cache) == true)
            {
                cache.Object.SetActive(true);
                cache.Transform.parent = null;
                cache.Transform.SetPositionAndRotation(position, rotation);

                return cache;
            }

            if (_prefabs.TryGetValue(type, out GameObject prefab) == true)
            {
                GameObject meteor = _container.InstantiatePrefab(prefab, position, rotation, null);

                if (meteor.TryGetComponent(out DamageDealer damageDealer) == false) throw new MissingComponentException($"{typeof(DamageDealer)}");
                if (meteor.TryGetComponent(out IMovementBehaviourSupplier supplier) == false) throw new MissingComponentException($"{typeof(IMovementBehaviourSupplier)}");
                if (meteor.TryGetComponent(out IEntityView view) == false) throw new MissingComponentException($"{typeof(IEntityView)}");

                return new(meteor, damageDealer, supplier, view);
            }

            throw new MissingMemberException($"There is no meteor prefab of type {type} in the config!");
        }

        public void Release(MeteorType type, MeteorCache meteor)
        {
            if (meteor is null) throw new ArgumentNullException();

            meteor.Object.SetActive(false);
            meteor.Transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            meteor.Transform.parent = _objectPoolsAnchors[type];
            meteor.Transform.localScale = Vector3.one;

            if (_objectPools.TryGetValue(type, out Stack<MeteorCache> pool) == true)
            {
                pool.Push(meteor);
            }
            else
            {
                Stack<MeteorCache> newPool = new();
                newPool.Push(meteor);

                _objectPools.Add(type, newPool);
            }
        }
    }
}