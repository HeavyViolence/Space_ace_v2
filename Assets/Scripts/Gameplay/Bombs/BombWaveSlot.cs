using SpaceAce.Main.Factories.BombFactories;

namespace SpaceAce.Gameplay.Bombs
{
    public readonly struct BombWaveSlot
    {
        public BombSize Size { get; }
        public float SpawnDelay { get; }
        public float Speed { get; }

        public BombWaveSlot(BombSize size,
                            float spawnDelay,
                            float speed)
        {
            Size = size;
            SpawnDelay = spawnDelay;
            Speed = speed;
        }
    }
}