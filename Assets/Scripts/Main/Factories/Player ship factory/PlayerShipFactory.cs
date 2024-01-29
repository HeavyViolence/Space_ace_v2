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
            _diContainer = container ?? throw new ArgumentNullException();

            if (slots is not null)
            {
                foreach (var slot in slots)
                    if (_playerShips.ContainsKey(slot.ShipType) == false)
                        _playerShips.Add(slot.ShipType, slot.Prefab);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public GameObject Create(PlayerShipType type, Vector3 position, Quaternion rotation)
        {
            if (_objectPool.TryGetValue(type, out var ship) == true)
            {
                ship.SetActive(true);
                ship.transform.SetPositionAndRotation(position, rotation);

                return ship;
            }

            return _diContainer.InstantiatePrefab(_playerShips[type], position, rotation, null);
        }

        public void Release(GameObject instance, PlayerShipType type)
        {
            if (instance == null) throw new ArgumentNullException();

            instance.SetActive(false);
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (_objectPool.TryAdd(type, instance) == false) UnityEngine.Object.Destroy(instance);
        }
    }
}