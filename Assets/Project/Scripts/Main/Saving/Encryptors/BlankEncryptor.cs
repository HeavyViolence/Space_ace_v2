namespace SpaceAce.Main.Saving
{
    public sealed class BlankEncryptor : IEncryptor
    {
        public byte[] Encrypt(byte[] data, byte[] key) => data;
        public byte[] Decrypt(byte[] data, byte[] key) => data;
    }
}