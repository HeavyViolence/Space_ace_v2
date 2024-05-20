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
        private readonly Dictionary<PlayerShipType, Stack<ShipCache>> _objectPool = new();
        private readonly Dictionary<PlayerShipType, Transform> _objectPoolsAnchors = new();
        private readonly GameObject _masterAnchor = new("Player ship object pools");

        public PlayerShipFactory(IEnumerable<KeyValuePair<PlayerShipType, GameObject>> prefabs, DiContainer container)
        {
            if (prefabs is null) throw new ArgumentNullException();
            _shipPrefabs = new(prefabs);

            _diContainer = container ?? throw new ArgumentNullException();

            BuildObjectPoolsAnchors();
        }

        private void BuildObjectPoolsAnchors()
        {
            Array types = Enum.GetValues(typeof(PlayerShipType));

            foreach (PlayerShipType type in types)
            {
                GameObject anchor = new($"{type.ToString().ToLower()}");
                anchor.transform.parent = _masterAnchor.transform;

                _objectPoolsAnchors.Add(type, anchor.transform);
            }
        }

        public ShipCache Create(PlayerShipType type, Vector3 position, Quaternion rotation)
        {
            if (_objectPool.TryGetValue(type, out Stack<ShipCache> pool) == true &&
                pool.TryPop(out ShipCache ship) == true)
            {
                ship.Object.SetActive(true);
                ship.Transform.SetPositionAndRotation(position, rotation);
                ship.Transform.parent = null;

                return ship;
            }

            if (_shipPrefabs.TryGetValue(type, out GameObject prefab) == true)
            {
                GameObject shipObject = _diContainer.InstantiatePrefab(prefab, position, rotation, null);
                shipObject.SetActive(true);

                if (shipObject.TryGetComponent(out Durability durability) == false) throw new MissingComponentException(nameof(Durability));
                if (shipObject.TryGetComponent(out Armor armor) == false) throw new MissingComponentException(nameof(Armor));
                if (shipObject.TryGetComponent(out Shooting shooting) == false) throw new MissingComponentException(nameof(Shooting));
                if (shipObject.TryGetComponent(out IMovementController movement) == false) throw new MissingComponentException(nameof(IMovementController));
                if (shipObject.TryGetComponent(out IDamageable damageable) == false) throw new MissingComponentException(nameof(IDamageable));
                if (shipObject.TryGetComponent(out IDestroyable destroyable) == false) throw new MissingComponentException(nameof(IDestroyable));
                if (shipObject.TryGetComponent(out IEscapable escapable) == false) throw new MissingComponentException(nameof(IEscapable));

                ShipCache cache = new(shipObject, durability, armor, shooting, movement, damageable, destroyable, escapable);

                return cache;
            }

            throw new MissingMemberException($"There is no player ship prefab of type {type} in the config!");
        }

        public void Release(PlayerShipType type, ShipCache cache)
        {
            if (cache is null) throw new ArgumentNullException();

            cache.Object.SetActive(false);
            cache.Transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            cache.Transform.parent = _objectPoolsAnchors[type];

            if (_objectPool.TryGetValue(type, out Stack<ShipCache> pool) == true)
            {
                pool.Push(cache);
            }
            else
            {
                Stack<ShipCache> newPool = new();
                newPool.Push(cache);

                _objectPool.Add(type, newPool);
            }
        }
    }
}