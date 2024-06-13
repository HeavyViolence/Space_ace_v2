using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SpaceAce.Main.Saving
{
    public sealed class RandomXOREncryptor : IEncryptor
    {
        private readonly RandomNumberGenerator _RNG = RandomNumberGenerator.Create();

        public byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data is null) throw new ArgumentNullException();
            if (key is null) throw new ArgumentNullException();

            if (data.Length == 0 || key.Length == 0) return data;

            byte[] iv = new byte[key.Length];
            _RNG.GetNonZeroBytes(iv);

            byte[] randomizedKey = new byte[key.Length];

            for (int i = 0; i < key.Length; i++)
                randomizedKey[i] = (byte)(key[i] ^ iv[i % iv.Length]);

            List<byte> output = new(data.Length + iv.Length);

            for (int i = 0; i < data.Length; i++)
                output.Add((byte)(data[i] ^ randomizedKey[i % randomizedKey.Length]));

            output.AddRange(iv);

            return output.ToArray();
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            if (data is null) throw new ArgumentNullException();
            if (key is null) throw new ArgumentNullException();

            if (data.Length == 0 || key.Length == 0) return data;

            int firstIVBytePosition = data.Length - key.Length;
            byte[] iv = data[firstIVBytePosition..];
            byte[] randomizedKey = new byte[key.Length];
            byte[] encryptedData = data[..firstIVBytePosition];
            byte[] output = new byte[encryptedData.Length];

            for (int i = 0; i < key.Length; i++)
                randomizedKey[i] = (byte)(key[i] ^ iv[i % iv.Length]);

            for (int i = 0; i < encryptedData.Length; i++)
                output[i] = (byte)(encryptedData[i] ^ randomizedKey[i % randomizedKey.Length]);

            return output;
        }
    }
}