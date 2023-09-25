namespace SpaceAce.Main.Saving
{
    public sealed class BlankKeyGenerator : IKeyGenerator
    {
        public byte[] GenerateKey(KeyStrength strenght, int seed = 0) => new byte[(int)strenght];
    }
}