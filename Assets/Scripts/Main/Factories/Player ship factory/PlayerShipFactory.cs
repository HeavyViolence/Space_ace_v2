using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.UI;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.PlayerShipFactories
{
    public sealed class PlayerShipFactory
    {
        private readonly Dictionary<PlayerShipType, GameObject> _shipPrefabs;
        private readonly DiContainer _diContainer;

        public PlayerShipFactory(IEnumerable<KeyValuePair<PlayerShipType, GameObject>> prefabs, DiContainer container)
        {
            if (prefabs is null) throw new ArgumentNullException();
            _shipPrefabs = new(prefabs);

            _diContainer = container ?? throw new ArgumentNullException();
        }

        public ShipCache Create(PlayerShipType type, Vector3 position, Quaternion rotation)
        {
            if (_shipPrefabs.TryGetValue(type, out GameObject prefab) == true)
            {
                GameObject ship = _diContainer.InstantiatePrefab(prefab, position, rotation, null);
                ship.SetActive(true);

                if (ship.TryGetComponent(out Durability durability) == false) throw new MissingComponentException(nameof(Durability));
                if (ship.TryGetComponent(out Armor armor) == false) throw new MissingComponentException(nameof(Armor));
                if (ship.TryGetComponent(out Shooting shooting) == false) throw new MissingComponentException(nameof(Shooting));
                if (ship.TryGetComponent(out IMovementController movement) == false) throw new MissingComponentException(nameof(IMovementController));
                if (ship.TryGetComponent(out IDamageable damageable) == false) throw new MissingComponentException(nameof(IDamageable));
                if (ship.TryGetComponent(out IEntityView view) == false) throw new MissingComponentException(nameof(IEntityView));

                return new(ship, durability, armor, shooting, movement, damageable, view);
            }

            throw new Exception($"There is no player ship prefab of type {type} in the config!");
        }
    }
}