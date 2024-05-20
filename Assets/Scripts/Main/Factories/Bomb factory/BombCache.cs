using SpaceAce.Gameplay.Movement;
using SpaceAce.UI;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.BombFactories
{
    public sealed record BombCache
    {
        public GameObject Object { get; }
        public Transform Transform { get; }
        public IMovementBehaviourSupplier MovementSupplier { get; }
        public IEntityView View { get; }

        public BombCache(GameObject instance,
                         IMovementBehaviourSupplier supplier,
                         IEntityView view)
        {
            if (instance == null) throw new ArgumentNullException();
            Object = instance;

            Transform = instance.transform;

            MovementSupplier = supplier ?? throw new ArgumentNullException();
            View = view ?? throw new ArgumentNullException();
        }
    }
}