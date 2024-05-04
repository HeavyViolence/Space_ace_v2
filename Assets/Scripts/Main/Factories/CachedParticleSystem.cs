using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    public readonly struct CachedParticleSystem
    {
        public GameObject Object { get; }
        public Transform Transform { get; }
        public ParticleSystemPauser Pauser { get; }

        public CachedParticleSystem(GameObject instance,
                                    Transform transform,
                                    ParticleSystemPauser pauser)
        {
            if (instance == null) throw new ArgumentNullException();
            Object = instance;

            if (transform == null) throw new ArgumentNullException();
            Transform = transform;

            if (pauser == null) throw new ArgumentNullException();
            Pauser = pauser;
        }
    }
}