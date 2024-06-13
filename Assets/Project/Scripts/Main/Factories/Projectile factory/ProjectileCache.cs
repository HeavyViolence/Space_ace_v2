using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.ProjectileFactories
{
    public sealed record ProjectileCache
    {
        public GameObject Object { get; }
        public Transform Transform { get; }
        public SpriteRenderer SpriteRenderer { get; }
        public DamageDealer DamageDealer { get; }
        public IEscapable Escapable { get; }
        public IMovementBehaviourSupplier MovementBehaviourSupplier { get; }

        public ProjectileCache(GameObject instance,
                               SpriteRenderer spriteRenderer,
                               DamageDealer damageDealer,
                               IEscapable escapable,
                               IMovementBehaviourSupplier behaviourSupplier)
        {
            if (instance == null) throw new ArgumentNullException();
            Object = instance;

            Transform = instance.transform;

            if (spriteRenderer == null) throw new ArgumentNullException();
            SpriteRenderer = spriteRenderer;

            if (damageDealer == null) throw new ArgumentNullException();
            DamageDealer = damageDealer;

            Escapable = escapable ?? throw new ArgumentNullException();
            MovementBehaviourSupplier = behaviourSupplier ?? throw new ArgumentNullException();
        }
    }
}