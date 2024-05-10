using SpaceAce.Main.Factories.WreckFactories;

using UnityEngine;

namespace SpaceAce.Gameplay.Wrecks
{
    public readonly struct WreckWaveSlot
    {
        public WreckType Type { get; }
        public Vector3 Scale { get; }
        public float SpawnDelay { get; }
        public float Speed { get; }

        public WreckWaveSlot(WreckType type,
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