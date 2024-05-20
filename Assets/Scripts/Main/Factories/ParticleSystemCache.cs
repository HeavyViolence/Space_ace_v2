using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    public record ParticleSystemCache
    {
        public GameObject Object { get; }
        public Transform Transform { get; }
        public ParticleSystemPauser Pauser { get; }

        public ParticleSystemCache(GameObject instance,
                                    ParticleSystemPauser pauser)
        {
            if (instance == null) throw new ArgumentNullException();
            Object = instance;
            Transform = instance.transform;

            if (pauser == null) throw new ArgumentNullException();
            Pauser = pauser;
        }
    }
}