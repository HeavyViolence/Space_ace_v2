using System;

namespace SpaceAce.Main.Saving
{
    public sealed class XOREncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data is null || data.Length == 0) throw new ArgumentNullException(nameof(data), "Attempted to pass an empty data to encrypt!");
            if (key is null || key.Length == 0) throw new ArgumentNullException(nameof(key), "Attempted to pass an empty encryption key!");

            byte[] encryptedData = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                byte encryptedByte = (byte)(data[i] ^ key[i % key.Length]);
                encryptedData[i] = encryptedByte;
            }

            return encryptedData;
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            if (data is null || data.Length == 0) throw new ArgumentNullException(nameof(data), "Attempted to pass an empty data to decrypt!");
            if (key is null || key.Length == 0) throw new ArgumentNullException(nameof(key), "Attempted to pass an empty decryption key!");

            byte[] decryptedData = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                byte decryptedByte = (byte)(data[i] ^ key[i % key.Length]);
                decryptedData[i] = decryptedByte;
            }

            return decryptedData;
        }
    }
}