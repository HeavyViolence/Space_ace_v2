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
            if (instance == null)
                throw new ArgumentNullException(nameof(instance),
                    $"Attempted to pass an empty instance of {typeof(GameObject)}!");

            Instance = instance;

            if (pauser == null)
                throw new ArgumentNullException(nameof(pauser),
                    $"Attempted to pass an empty {typeof(ParticleSystemPauser)}!");

            Pauser = pauser;
        }
    }
}