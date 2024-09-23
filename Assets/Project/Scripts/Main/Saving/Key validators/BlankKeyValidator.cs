namespace SpaceAce.Main.Saving
{
    public sealed class BlankKeyValidator : IKeyValidator
    {
        public bool IsKeyValid(byte[] key) => true;
    }
}