using System;
using System.Security.Cryptography;

namespace SpaceAce.Main.Saving
{
    public sealed class RandomXOREncryptor : IEncryptor
    {
        private readonly RandomNumberGenerator _RNG = RandomNumberGenerator.Create();

        public byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data is null || data.Length == 0)
                throw new ArgumentNullException(nameof(data),
                    "Attempted to pass an empty data to encrypt!");

            if (key is null || key.Length != IKeyGenerator.ByteKeyLength)
                throw new ArgumentNullException(nameof(key),
                    "Encryption key is empty or has an invalid length!");

            byte[] iv = new byte[IKeyGenerator.ByteKeyLength];
            _RNG.GetNonZeroBytes(iv);

            byte[] output = new byte[data.Length + iv.Length];

            for (int i = 0; i < data.Length; i++)
                output[i] = (byte)(data[i] ^ key[i % key.Length] ^ iv[i % iv.Length]);

            for (int i = 0; i < iv.Length; i++)
                output[output.Length + i] = iv[i];

            return output;
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            if (data is null || data.Length <= IKeyGenerator.ByteKeyLength)
                throw new ArgumentNullException(nameof(data),
                    "Attempted to pass an empty data to decrypt!");

            if (key is null || key.Length != IKeyGenerator.ByteKeyLength)
                throw new ArgumentNullException(nameof(key),
                    "Decryption key is empty or has an invalid length!");

            int firstIVBytePosition = data.Length - IKeyGenerator.ByteKeyLength;

            byte[] iv = data[firstIVBytePosition..];
            byte[] encryptedData = data[..firstIVBytePosition];
            byte[] output = new byte[encryptedData.Length];

            for (int i = 0; i < encryptedData.Length; i++)
                output[i] = (byte)(encryptedData[i] ^ key[i % key.Length] ^ iv[i % iv.Length]);

            return output;
        }
    }
}