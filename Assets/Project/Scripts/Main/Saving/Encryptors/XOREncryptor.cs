using System;

namespace SpaceAce.Main.Saving
{
    public sealed class XOREncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data is null) throw new ArgumentNullException();
            if (key is null) throw new ArgumentNullException();

            if (data.Length == 0 || key.Length == 0) return data;

            byte[] output = new byte[data.Length];

            for (int i  = 0; i < data.Length; i++)
                output[i] = (byte)(data[i] ^ key[i % key.Length]);

            return output;
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            if (data is null) throw new ArgumentNullException();
            if (key is null) throw new ArgumentNullException();

            if (data.Length == 0 || key.Length == 0) return data;

            byte[] output = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                output[i] = (byte)(data[i] ^ key[i % key.Length]);

            return output;
        }
    }
}