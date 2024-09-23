using SpaceAce.Auxiliary;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SpaceAce.Main.Saving
{
    public sealed class RandomXOREncryptor : Encryptor
    {
        private readonly RandomNumberGenerator _RNG = RandomNumberGenerator.Create();

        public RandomXOREncryptor(IKeyValidator validator) : base(validator) { }

        public override byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data is null) throw new ArgumentNullException();
            if (KeyValidator.IsKeyValid(key) == false) throw new ArgumentException();

            byte[] iv = new byte[key.Length];
            _RNG.GetNonZeroBytes(iv);

            byte[] randomizedKey = AuxMath.XOR(key, iv);

            List<byte> output = new(data.Length + iv.Length);

            for (int i = 0; i < data.Length; i++)
            {
                int b = data[i] ^ randomizedKey[i % randomizedKey.Length];
                output.Add((byte)b);

                if (i % randomizedKey.Length == 0)
                    AuxMath.PrimeTransform(randomizedKey);
            }

            output.AddRange(iv);

            return output.ToArray();
        }

        public override byte[] Decrypt(byte[] data, byte[] key)
        {
            if (data is null) throw new ArgumentNullException();
            if (KeyValidator.IsKeyValid(key) == false) throw new ArgumentException();

            int firstIVBytePosition = data.Length - key.Length;
            byte[] iv = data[firstIVBytePosition..];
            byte[] randomizedKey = AuxMath.XOR(key, iv);
            byte[] encryptedData = data[..firstIVBytePosition];
            byte[] output = new byte[encryptedData.Length];

            for (int i = 0; i < encryptedData.Length; i++)
            {
                int b = encryptedData[i] ^ randomizedKey[i % randomizedKey.Length];
                output[i] = (byte)b;

                if (i % randomizedKey.Length == 0)
                    AuxMath.PrimeTransform(randomizedKey);
            }

            return output;
        }
    }
}