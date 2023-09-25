namespace SpaceAce.Main.Saving
{
    public interface IEncryptor
    {
        byte[] Encrypt(byte[] data, byte[] key);
        byte[] Decrypt(byte[] data, byte[] key);
    }
}