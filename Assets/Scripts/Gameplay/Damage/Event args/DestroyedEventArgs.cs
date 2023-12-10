using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class DestroyedEventArgs : EventArgs
    {
        public Vector3 DeathPosition { get; }
        public float Lifetime { get; }
        public float ExperienceEarned { get; }
        public float ExperienceLost { get; }
        public float ExperienceTotal => ExperienceEarned + ExperienceLost;
        public float ExperienceEfficiency => ExperienceEarned / ExperienceTotal;

        public DestroyedEventArgs(Vector3 deathPosition,
                                  float lifetime,
                                  float experienceEarned,
                                  float experienceLost)
        {
            DeathPosition = deathPosition;
            Lifetime = lifetime;
            ExperienceEarned = experienceEarned;
            ExperienceLost = experienceLost;
        }
    }
}