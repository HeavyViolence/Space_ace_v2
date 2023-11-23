using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class PlayerShipFactory
    {
        private readonly DiContainer _diContainer;
        private readonly Dictionary<PlayerShipType, GameObject> _prefabs;
        private readonly Dictionary<PlayerShipType, GameObject> _objectPool = new();

        public PlayerShipFactory(DiContainer container, IEnumerable<KeyValuePair<PlayerShipType, GameObject>> prefabs)
        {
            _diContainer = container ?? throw new ArgumentNullException(nameof(container),
                $"Attempted to pass an empty DI container!");

            _prefabs = new Dictionary<PlayerShipType, GameObject>(prefabs) ?? throw new ArgumentNullException(nameof(prefabs),
                $"Attempted to pass an empty player ships prefabs!");
        }

        public GameObject CreatePlayerShip(PlayerShipType playerShipType)
        {
            if (_objectPool.TryGetValue(playerShipType, out var ship) == true) return ship;

            return _diContainer.InstantiatePrefab(_prefabs[playerShipType]);
        }

        public void ReleasePlayerShip(GameObject instance, PlayerShipType type)
        {
            instance.SetActive(false);
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            _objectPool.TryAdd(type, instance);
        }
    }
}