namespace SpaceAce.Main.Saving
{
    public sealed class BlankKeyGenerator : IKeyGenerator
    {
        public byte[] GenerateKey(ISavable entity) => new byte[IKeyGenerator.ByteKeyLength];
    }
}