using SpaceAce.Gameplay.Experience;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class DestroyedEventArgs : EventArgs
    {
        public Vector3 DeathPosition { get; }
        public float Lifetime { get; }
        public ExperienceDrop Experience { get; }

        public DestroyedEventArgs(Vector3 deathPosition,
                                  float lifetime,
                                  ExperienceDrop experience)
        {
            DeathPosition = deathPosition;
            Lifetime = lifetime;
            Experience = experience;
        }
    }
}