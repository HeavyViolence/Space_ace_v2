using System.Security.Cryptography;
using System.Text;

namespace SpaceAce.Main.Saving
{
    public sealed class RandomKeyGenerator : IKeyGenerator
    {
        private static readonly UTF8Encoding s_UTF8 = new(true, true);
        private static readonly SHA256 s_SHA256 = SHA256.Create();

        public byte[] GenerateKey(ISavable entity)
        {
            byte[] byteID = s_UTF8.GetBytes(entity.ID);
            byte[] key = s_SHA256.ComputeHash(byteID);

            return key;
        }
    }
}