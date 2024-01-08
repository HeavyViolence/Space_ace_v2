using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    public sealed class CachedProjectile
    {
        public GameObject Instance { get; }
        public SpriteRenderer SpriteRenderer { get; }
        public DamageDealer DamageDealer { get; }
        public IEscapable Escapable { get; }
        public IMovementBehaviourSupplier MovementBehaviourSupplier { get; }

        public CachedProjectile(GameObject instance,
                                SpriteRenderer spriteRenderer,
                                DamageDealer damageDealer,
                                IEscapable escapable,
                                IMovementBehaviourSupplier behaviourSupplier)
        {
            if (instance == null) throw new ArgumentNullException();
            Instance = instance;

            if (spriteRenderer == null) throw new ArgumentNullException();
            SpriteRenderer = spriteRenderer;

            if (damageDealer == null) throw new ArgumentNullException();
            DamageDealer = damageDealer;

            Escapable = escapable ?? throw new ArgumentNullException();
            MovementBehaviourSupplier = behaviourSupplier ?? throw new ArgumentNullException();
        }
    }
}