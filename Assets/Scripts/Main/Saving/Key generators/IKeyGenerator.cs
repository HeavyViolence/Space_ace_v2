namespace SpaceAce.Main.Saving
{
    public interface IKeyGenerator
    {
        const int ByteKeyLength = 32;

        byte[] GenerateKey(string id);
    }
}