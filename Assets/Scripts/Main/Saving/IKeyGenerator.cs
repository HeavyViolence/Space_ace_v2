namespace SpaceAce.Main.Saving
{
    public interface IKeyGenerator
    {
        byte[] GenerateKey(KeyStrength strength, int seed);
    }
}