using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    public sealed class CachedExplosion
    {
        public GameObject Explosion { get; }
        public ParticleSystemPauser Pauser { get; }

        public CachedExplosion(GameObject explosion,
                               ParticleSystemPauser pauser)
        {
            if (explosion == null)
                throw new ArgumentNullException(nameof(explosion),
                    $"Attempted to pass an empty explosion {typeof(GameObject)}!");

            Explosion = explosion;

            if (pauser == null)
                throw new ArgumentNullException(nameof(pauser),
                    $"Attempted to pass an empty {typeof(ParticleSystemPauser)}!");

            Pauser = pauser;
        }
    }
}