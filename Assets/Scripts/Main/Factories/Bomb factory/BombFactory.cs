using SpaceAce.Gameplay.Movement;
using SpaceAce.UI;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.BombFactories
{
    public sealed class BombFactory
    {
        private readonly Dictionary<BombSize, GameObject> _prefabs;
        private readonly Dictionary<BombSize, Stack<BombCache>> _objectPools = new();
        private readonly Dictionary<BombSize, Transform> _objectPoolsAnchors = new();
        private readonly Transform _masterAnchor = new GameObject("Bomb object pools").transform;
        private readonly DiContainer _container;

        public BombFactory(IEnumerable<KeyValuePair<BombSize, GameObject>> prefabs, DiContainer container)
        {
            if (prefabs is null) throw new ArgumentNullException();
            _prefabs = new(prefabs);

            _container = container ?? throw new ArgumentNullException();

            BuildObjectPoolsAnchors();
        }

        private void BuildObjectPoolsAnchors()
        {
            Array sizes = Enum.GetValues(typeof(BombSize));

            foreach (BombSize size in sizes)
            {
                Transform anchor = new GameObject($"{size}").transform;
                anchor.parent = _masterAnchor;

                _objectPoolsAnchors.Add(size, anchor);
            }
        }

        public BombCache Create(BombSize size, Vector3 position, Quaternion rotation)
        {
            if (_objectPools.TryGetValue(size, out Stack<BombCache> pool) == true &&
                pool.TryPop(out BombCache cache) == true)
            {
                cache.Object.SetActive(true);
                cache.Transform.parent = null;
                cache.Transform.SetPositionAndRotation(position, rotation);

                return cache;
            }

            if (_prefabs.TryGetValue(size, out GameObject prefab) == true)
            {
                GameObject bomb = _container.InstantiatePrefab(prefab, position, rotation, null);

                if (bomb.TryGetComponent(out IMovementBehaviourSupplier supplier) == false) throw new MissingComponentException($"{typeof(IMovementBehaviourSupplier)}");
                if (bomb.TryGetComponent(out IEntityView view) == false) throw new MissingComponentException($"{typeof(IEntityView)}");

                return new(bomb, supplier, view);
            }

            throw new Exception($"There is no bomb prefab of size {size} in the config!");
        }

        public void Release(BombSize size, BombCache bomb)
        {
            if (bomb is null) throw new ArgumentNullException();

            bomb.Object.SetActive(false);
            bomb.Transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            bomb.Transform.parent = _objectPoolsAnchors[size];

            if (_objectPools.TryGetValue(size, out Stack<BombCache> pool) == true)
            {
                pool.Push(bomb);
            }
            else
            {
                Stack<BombCache> newPool = new();
                newPool.Push(bomb);

                _objectPools.Add(size, newPool);
            }
        }
    }
}