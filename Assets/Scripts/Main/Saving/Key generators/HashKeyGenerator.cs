using System;
using System.Security.Cryptography;
using System.Text;

namespace SpaceAce.Main.Saving
{
    public sealed class HashKeyGenerator : IKeyGenerator
    {
        private readonly UTF8Encoding _UTF8 = new(true, true);
        private readonly SHA256 _SHA256 = SHA256.Create();

        public byte[] GenerateKey(string id)
        {
            if (string.IsNullOrEmpty(id) ||
                string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id),
                    "Attempted to pass an empty ID!");

            byte[] byteID = _UTF8.GetBytes(id);
            byte[] key = _SHA256.ComputeHash(byteID);

            return key;
        }
    }
}