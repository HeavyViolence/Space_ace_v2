namespace SpaceAce.Main.Saving
{
    public interface IKeyGenerator
    {
        byte[] GenerateKey(string id);
    }
}