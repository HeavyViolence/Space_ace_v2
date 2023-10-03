using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SpaceAce.Main.Saving
{
    public sealed class AESEncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data is null || data.Length == 0) throw new ArgumentNullException(nameof(data), "Attempted to pass an empty data to encrypt!");
            if (key is null || key.Length == 0) throw new ArgumentNullException(nameof(key), "Attempted to pass an empty encryption key!");

            foreach (var validLength in Enum.GetValues(typeof(KeyStrength)))
                if (key.Length % (int)validLength != 0)
                    throw new ArgumentException("Attempted to pass encryption key of invalid length!", nameof(key));

            using Aes algorithm = Aes.Create();
            using ICryptoTransform encryptor = algorithm.CreateEncryptor(key, algorithm.IV);

            byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

            List<byte> result = new(data.Length + algorithm.IV.Length);
            result.AddRange(encryptedData);
            result.AddRange(algorithm.IV);

            algorithm.Clear();

            return result.ToArray();
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            if (data is null || data.Length == 0) throw new ArgumentNullException(nameof(data), "Attempted to pass an empty data to decrypt!");
            if (key is null) throw new ArgumentNullException(nameof(key), "Attempted to pass an empty decryption key!");

            foreach (var validLength in Enum.GetValues(typeof(KeyStrength)))
                if (key.Length % (int)validLength != 0)
                    throw new ArgumentException("Attempted to pass decryption key of invalid length!", nameof(key));

            using Aes algorithm = Aes.Create();

            int firstIVBytePosition = data.Length - algorithm.IV.Length;
            byte[] encryptedData = data[..firstIVBytePosition];
            byte[] iv = data[firstIVBytePosition..];

            using ICryptoTransform decryptor = algorithm.CreateDecryptor(key, iv);
            byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            algorithm.Clear();

            return decryptedData;
        }
    }
}