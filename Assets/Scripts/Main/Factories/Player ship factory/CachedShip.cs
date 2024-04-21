using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    public readonly struct CachedShip
    {
        public GameObject Ship { get; }
        public Durability Durability { get; }
        public Armor Armor { get; }
        public IMovementController Movement { get; }
        public Shooting Shooting { get; }
        public IDamageable Damageable { get; }
        public IDestroyable Destroyable { get; }
        public IEscapable Escapable { get; }

        public bool Incomplete => Ship == null ||
                                  Durability == null ||
                                  Armor == null ||
                                  Movement is null ||
                                  Shooting == null ||
                                  Damageable is null ||
                                  Destroyable is null ||
                                  Escapable is null;

        public CachedShip(GameObject ship,
                          Durability durability,
                          Armor armor,
                          IMovementController movement,
                          Shooting shooting,
                          IDamageable damageable,
                          IDestroyable destroyable,
                          IEscapable escapable)
        {
            if (ship == null) throw new ArgumentNullException();
            Ship = ship;

            if (durability == null) throw new ArgumentNullException();
            Durability = durability;

            if (armor == null) throw new ArgumentNullException();
            Armor = armor;

            Movement = movement ?? throw new ArgumentNullException();

            if (shooting == null) throw new ArgumentNullException();
            Shooting = shooting;

            Damageable = damageable ?? throw new ArgumentNullException();
            Destroyable = destroyable ?? throw new ArgumentNullException();
            Escapable = escapable ?? throw new ArgumentNullException();
        }
    }
}