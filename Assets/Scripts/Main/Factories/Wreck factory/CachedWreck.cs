using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;
using SpaceAce.UI;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.WreckFactories
{
    public readonly struct CachedWreck
    {
        public GameObject Object { get; }
        public Transform Transform { get; }
        public DamageDealer DamageDealer { get; }
        public IMovementBehaviourSupplier MovementSupplier { get; }
        public IEntityView View { get; }

        public CachedWreck(GameObject obj,
                           Transform transform,
                           DamageDealer damageDealer,
                           IMovementBehaviourSupplier supplier,
                           IEntityView view)
        {
            if (obj == null) throw new ArgumentNullException();
            Object = obj;

            if (transform == null) throw new ArgumentNullException();
            Transform = transform;

            if (damageDealer == null) throw new ArgumentNullException();
            DamageDealer = damageDealer;

            MovementSupplier = supplier ?? throw new ArgumentNullException();
            View = view ?? throw new ArgumentNullException();
        }
    }
}