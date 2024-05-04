using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.PlayerShipFactories
{
    public sealed class PlayerShipFactory
    {
        private readonly DiContainer _diContainer;
        private readonly Dictionary<PlayerShipType, GameObject> _shipPrefabs = new();
        private readonly Dictionary<PlayerShipType, Stack<CachedShip>> _objectPool = new();

        public PlayerShipFactory(DiContainer container, IEnumerable<PlayerShipSlot> slots)
        {
            _diContainer = container ?? throw new ArgumentNullException();

            if (slots is not null)
            {
                foreach (var slot in slots)
                    if (_shipPrefabs.ContainsKey(slot.Type) == false)
                        _shipPrefabs.Add(slot.Type, slot.Prefab);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public CachedShip Create(PlayerShipType type, Vector3 position, Quaternion rotation)
        {
            if (_objectPool.TryGetValue(type, out Stack<CachedShip> pool) == true &&
                pool.TryPop(out CachedShip ship) == true)
            {
                ship.Ship.SetActive(true);
                ship.Ship.transform.SetPositionAndRotation(position, rotation);

                return ship;
            }
            else
            {
                GameObject shipObject = _diContainer.InstantiatePrefab(_shipPrefabs[type], position, rotation, null);

                if (shipObject.TryGetComponent(out Durability durability) == false) throw new MissingComponentException(nameof(Durability));
                if (shipObject.TryGetComponent(out Armor armor) == false) throw new MissingComponentException(nameof(Armor));
                if (shipObject.TryGetComponent(out Shooting shooting) == false) throw new MissingComponentException(nameof(Shooting));
                if (shipObject.TryGetComponent(out IMovementController movement) == false) throw new MissingComponentException(nameof(IMovementController));
                if (shipObject.TryGetComponent(out IDamageable damageable) == false) throw new MissingComponentException(nameof(IDamageable));
                if (shipObject.TryGetComponent(out IDestroyable destroyable) == false) throw new MissingComponentException(nameof(IDestroyable));
                if (shipObject.TryGetComponent(out IEscapable escapable) == false) throw new MissingComponentException(nameof(IEscapable));

                CachedShip cache = new(shipObject, durability, armor, shooting, movement, damageable, destroyable, escapable);

                shipObject.SetActive(true);
                shipObject.transform.SetPositionAndRotation(position, rotation);

                return cache;
            }
        }

        public void Release(CachedShip cache, PlayerShipType type)
        {
            if (cache.Incomplete == true) throw new ArgumentNullException();

            cache.Ship.SetActive(false);
            cache.Ship.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (_objectPool.TryGetValue(type, out Stack<CachedShip> pool) == true)
            {
                pool.Push(cache);
            }
            else
            {
                Stack<CachedShip> newPool = new();
                newPool.Push(cache);

                _objectPool.Add(type, newPool);
            }
        }
    }
}