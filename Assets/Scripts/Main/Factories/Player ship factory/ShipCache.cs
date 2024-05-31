using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.UI;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.PlayerShipFactories
{
    public sealed record ShipCache
    {
        public GameObject Object { get; }
        public Transform Transform { get; }
        public Durability Durability { get; }
        public Armor Armor { get; }
        public Shooting Shooting { get; }
        public IMovementController Movement { get; }
        public IDamageable Damageable { get; }
        public IEntityView View { get; }

        public ShipCache(GameObject instance,
                         Durability durability,
                         Armor armor,
                         Shooting shooting,
                         IMovementController movement,
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

            Movement = movement ?? throw new ArgumentNullException();
            Damageable = damageable ?? throw new ArgumentNullException();
            View = view ?? throw new ArgumentNullException();
        }
    }
}