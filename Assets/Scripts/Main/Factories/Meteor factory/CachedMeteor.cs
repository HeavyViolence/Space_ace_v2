using SpaceAce.Gameplay.Movement;
using SpaceAce.UI;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.MeteorFactories
{
    public readonly struct CachedMeteor
    {
        public GameObject Object { get; }
        public Transform Transform { get; }
        public IMovementBehaviourSupplier MovementSupplier { get; }
        public IEntityView View { get; }

        public CachedMeteor(GameObject obj,
                            Transform transform,
                            IMovementBehaviourSupplier supplier,
                            IEntityView view)
        {
            if (obj == null) throw new ArgumentNullException();
            Object = obj;

            if (transform == null) throw new ArgumentNullException();
            Transform = transform;

            MovementSupplier = supplier ?? throw new ArgumentNullException();
            View = view ?? throw new ArgumentNullException();
        }
    }
}