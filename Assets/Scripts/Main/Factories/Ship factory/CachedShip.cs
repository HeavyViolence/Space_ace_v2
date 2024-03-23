using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    public readonly struct CachedShip
    {
        public GameObject Ship { get; }
        public IMovementController Movement { get; }
        public Shooting Shooting { get; }
        public IDamageable Damageable { get; }
        public IDestroyable Destroyable { get; }

        public bool Incomplete => Ship == null ||
                                  Movement is null ||
                                  Shooting == null ||
                                  Damageable is null ||
                                  Destroyable is null;

        public CachedShip(GameObject ship,
                          IMovementController movement,
                          Shooting shooting,
                          IDamageable damageable,
                          IDestroyable destroyable)
        {
            if (ship == null) throw new ArgumentNullException();
            Ship = ship;

            Movement = movement ?? throw new ArgumentNullException();

            if (shooting == null) throw new ArgumentNullException();
            Shooting = shooting;

            Damageable = damageable ?? throw new ArgumentNullException();
            Destroyable = destroyable ?? throw new ArgumentNullException();
        }
    }
}