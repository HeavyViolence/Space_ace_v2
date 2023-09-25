using UnityEngine;

namespace SpaceAce.Gameplay.Players
{
    public sealed class PlayerState
    {
        public PlayerState Default => new(0f, 0f);

        public float Credits { get; }
        public float Experience { get; }

        public PlayerState(float credits,
                           float experience)
        {
            Credits = Mathf.Clamp(credits, 0f, float.MaxValue);
            Experience = Mathf.Clamp(experience, 0f, float.MaxValue);
        }
    }
}