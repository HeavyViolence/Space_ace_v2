using SpaceAce.Auxiliary;

using System;

namespace SpaceAce.Main.Saving
{
    public sealed class XOREncryptor : Encryptor
    {
        public XOREncryptor(IKeyValidator validator) : base(validator) { }

        public override byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data is null) throw new ArgumentNullException();
            if (KeyValidator.IsKeyValid(key) == false) throw new ArgumentException();

            return AuxMath.XOR(data, key);
        }

        public override byte[] Decrypt(byte[] data, byte[] key)
        {
            if (data is null) throw new ArgumentNullException();
            if (KeyValidator.IsKeyValid(key) == false) throw new ArgumentException();

            return AuxMath.XOR(data, key);
        }
    }
}