using System;

namespace SpaceAce.Main.Saving
{
    public sealed class BlankEncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data, byte[] key = null)
        {
            if (data is null || data.Length == 0) throw new ArgumentNullException(nameof(data), "Attempted to pass an empty data to encrypt!");
            return data;
        }

        public byte[] Decrypt(byte[] data, byte[] key = null)
        {
            if (data is null || data.Length == 0) throw new ArgumentNullException(nameof(data), "Attempted to pass an empty data to decrypt!");
            return data;
        }
    }
}