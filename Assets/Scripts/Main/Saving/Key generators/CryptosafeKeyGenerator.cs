using System.Security.Cryptography;

namespace SpaceAce.Main.Saving
{
    public sealed class CryptosafeKeyGenerator : IKeyGenerator
    {
        private static readonly RandomNumberGenerator s_generator = RandomNumberGenerator.Create();

        public byte[] GenerateKey(KeyStrength strength, int seed = 0)
        {
            byte[] key = new byte[(int)strength];
            s_generator.GetNonZeroBytes(key);

            return key;
        }
    }
}