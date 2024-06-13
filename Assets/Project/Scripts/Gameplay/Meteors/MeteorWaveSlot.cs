using SpaceAce.Main.Factories.MeteorFactories;

using UnityEngine;

namespace SpaceAce.Gameplay.Meteors
{
    public readonly struct MeteorWaveSlot
    {
        public MeteorType Type { get; }
        public Vector3 Scale { get; }
        public float SpawnDelay { get; }
        public float Speed { get; }
         
        public MeteorWaveSlot(MeteorType type,
                              Vector3 scale,
                              float spawnDelay,
                              float speed)
        {
            Type = type;
            Scale = scale;
            SpawnDelay = spawnDelay;
            Speed = speed;
        }
    }
}