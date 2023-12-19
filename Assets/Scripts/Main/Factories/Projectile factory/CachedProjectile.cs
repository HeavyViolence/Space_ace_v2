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
        public IMovementBehaviourSupplier BehaviourSupplier { get; }

        public CachedProjectile(GameObject instance,
                                SpriteRenderer spriteRenderer,
                                DamageDealer damageDealer,
                                IEscapable escapable,
                                IMovementBehaviourSupplier behaviourSupplier)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance),
                    "Attempted to pass an empty projectile prefab!");

            Instance = instance;

            if (spriteRenderer == null) throw new ArgumentNullException(nameof(spriteRenderer),
                $"Attempted to pass an empty {typeof(SpriteRenderer)}!");

            SpriteRenderer = spriteRenderer;

            if (damageDealer == null) throw new ArgumentNullException(nameof(damageDealer),
                $"Attempted to pass an empty {typeof(DamageDealer)}!");

            DamageDealer = damageDealer;

            Escapable = escapable ?? throw new ArgumentNullException(nameof(damageDealer),
                $"Attempted to pass an empty {typeof(IEscapable)}!");

            BehaviourSupplier = behaviourSupplier ?? throw new ArgumentNullException(nameof(behaviourSupplier),
                $"Attempted to pass an empty {typeof(IMovementBehaviourSupplier)}!");
        }
    }
}