namespace SpaceAce.Main.Saving
{
    public sealed class BlankKeyGenerator : IKeyGenerator
    {
        public byte[] GenerateKey(string id) => new byte[0];
    }
}