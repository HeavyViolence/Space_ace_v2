namespace SpaceAce.Main.Saving
{
    public sealed class BlankEncryptor : Encryptor
    {
        public BlankEncryptor(IKeyValidator validator) : base(validator) { }

        public override byte[] Encrypt(byte[] data, byte[] key) => data;
        public override byte[] Decrypt(byte[] data, byte[] key) => data;
    }
}