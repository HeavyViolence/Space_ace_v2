using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;
using SpaceAce.UI;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.WreckFactories
{
    public sealed record WreckCache
    {
        public GameObject Object { get; }
        public Transform Transform { get; }
        public DamageDealer DamageDealer { get; }
        public IMovementBehaviourSupplier MovementSupplier { get; }
        public IEntityView View { get; }

        public WreckCache(GameObject instance,
                          DamageDealer damageDealer,
                          IMovementBehaviourSupplier supplier,
                          IEntityView view)
        {
            if (instance == null) throw new ArgumentNullException();
            Object = instance;

            Transform = instance.transform;

            if (damageDealer == null) throw new ArgumentNullException();
            DamageDealer = damageDealer;

            MovementSupplier = supplier ?? throw new ArgumentNullException();
            View = view ?? throw new ArgumentNullException();
        }
    }
}