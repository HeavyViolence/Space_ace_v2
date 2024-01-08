using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    public sealed class CachedParticleSystem
    {
        public GameObject Instance { get; }
        public ParticleSystemPauser Pauser { get; }

        public CachedParticleSystem(GameObject instance,
                                    ParticleSystemPauser pauser)
        {
            if (instance == null) throw new ArgumentNullException();
            Instance = instance;

            if (pauser == null) throw new ArgumentNullException();
            Pauser = pauser;
        }
    }
}