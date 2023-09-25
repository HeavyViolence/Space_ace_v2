using System;

namespace SpaceAce.Main.Saving
{
    public sealed class RandomKeyGenerator : IKeyGenerator
    {
        public byte[] GenerateKey(KeyStrength strength, int seed)
        {
            Random generator = new(seed);
            byte[] key = new byte[(int)strength];

            generator.NextBytes(key);

            return key;
        }
    }
}