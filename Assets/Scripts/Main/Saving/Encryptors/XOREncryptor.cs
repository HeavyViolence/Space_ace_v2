using System;

namespace SpaceAce.Main.Saving
{
    public sealed class XOREncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data is null || data.Length == 0) throw new ArgumentNullException();
            if (key is null || key.Length != IKeyGenerator.ByteKeyLength) throw new ArgumentNullException();

            byte[] output = new byte[data.Length];

            for (int i  = 0; i < data.Length; i++)
                output[i] = (byte)(data[i] ^ key[i % key.Length]);

            return output;
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            if (data is null || data.Length == 0) throw new ArgumentNullException();
            if (key is null || key.Length != IKeyGenerator.ByteKeyLength) throw new ArgumentNullException();

            byte[] output = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                output[i] = (byte)(data[i] ^ key[i % key.Length]);

            return output;
        }
    }
}