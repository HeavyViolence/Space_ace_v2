using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class PlayerShipFactory
    {
        private readonly DiContainer _diContainer;
        private readonly Dictionary<PlayerShipType, GameObject> _playerShips = new();
        private readonly Dictionary<PlayerShipType, GameObject> _objectPool = new();

        public PlayerShipFactory(DiContainer container, IEnumerable<PlayerShipSlot> slots)
        {
            _diContainer = container ?? throw new ArgumentNullException(nameof(container),
                $"Attempted to pass an empty DI container!");

            if (slots is not null)
            {
                foreach (var slot in slots)
                    if (_playerShips.ContainsKey(slot.ShipType) == false)
                        _playerShips.Add(slot.ShipType, slot.Prefab);
            }
            else
            {
                throw new ArgumentNullException(nameof(slots),
                    $"Attempted to pass an empty player ships collection!");
            }
        }

        public GameObject Create(PlayerShipType type)
        {
            if (_objectPool.TryGetValue(type, out var ship) == true) return ship;

            return _diContainer.InstantiatePrefab(_playerShips[type]);
        }

        public void Release(GameObject instance, PlayerShipType type)
        {
            instance.SetActive(false);
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (_objectPool.TryAdd(type, instance) == false) UnityEngine.Object.Destroy(instance);
        }
    }
}