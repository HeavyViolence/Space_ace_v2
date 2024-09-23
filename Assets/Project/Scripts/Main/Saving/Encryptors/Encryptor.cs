using System;

namespace SpaceAce.Main.Saving
{
    public abstract class Encryptor
    {
        protected IKeyValidator KeyValidator { get; private set; }

        public Encryptor(IKeyValidator validator)
        {
            KeyValidator = validator ?? throw new ArgumentNullException();
        }

        public abstract byte[] Encrypt(byte[] data, byte[] key);
        public abstract byte[] Decrypt(byte[] data, byte[] key);
    }
}