using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.ProjectileFactories
{
    public readonly struct CachedProjectile
    {
        public GameObject Object { get; }
        public Transform Transform { get; }
        public SpriteRenderer SpriteRenderer { get; }
        public DamageDealer DamageDealer { get; }
        public IEscapable Escapable { get; }
        public IMovementBehaviourSupplier MovementBehaviourSupplier { get; }

        public bool Incomplete => Object == null ||
                                  Transform == null ||
                                  SpriteRenderer == null ||
                                  DamageDealer == null ||
                                  DamageDealer == null ||
                                  Escapable == null ||
                                  MovementBehaviourSupplier == null;

        public CachedProjectile(GameObject instance,
                                Transform transform,
                                SpriteRenderer spriteRenderer,
                                DamageDealer damageDealer,
                                IEscapable escapable,
                                IMovementBehaviourSupplier behaviourSupplier)
        {
            if (instance == null) throw new ArgumentNullException();
            Object = instance;

            if (transform == null) throw new ArgumentNullException();
            Transform = transform;

            if (spriteRenderer == null) throw new ArgumentNullException();
            SpriteRenderer = spriteRenderer;

            if (damageDealer == null) throw new ArgumentNullException();
            DamageDealer = damageDealer;

            Escapable = escapable ?? throw new ArgumentNullException();
            MovementBehaviourSupplier = behaviourSupplier ?? throw new ArgumentNullException();
        }
    }
}