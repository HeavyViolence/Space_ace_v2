using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.UI;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.PlayerShipFactories
{
    public sealed record ShipCache : IDisposable
    {
        public GameObject Object { get; private set; }
        public Transform Transform { get; private set; }
        public Durability Durability { get; private set; }
        public Armor Armor { get; private set; }
        public Shooting Shooting { get; private set; }
        public IMovementController MovementController { get; private set; }
        public IDamageable Damageable { get; private set; }
        public IEntityView View { get; private set; }

        public ShipCache(GameObject instance,
                         Durability durability,
                         Armor armor,
                         Shooting shooting,
                         IMovementController movementController,
                         IDamageable damageable,
                         IEntityView view)
        {
            if (instance == null) throw new ArgumentNullException();
            Object = instance;
            Transform = instance.transform;

            if (durability == null) throw new ArgumentNullException();
            Durability = durability;

            if (armor == null) throw new ArgumentNullException();
            Armor = armor;

            if (shooting == null) throw new ArgumentNullException();
            Shooting = shooting;

            MovementController = movementController ?? throw new ArgumentNullException();
            Damageable = damageable ?? throw new ArgumentNullException();
            View = view ?? throw new ArgumentNullException();
        }

        public void Dispose()
        {
            GameObject.Destroy(Object);

            Object = null;
            Transform = null;
            Durability = null;
            Armor = null;
            Shooting = null;
            MovementController = null;
            Damageable = null;
            View = null;
        }
    }
}