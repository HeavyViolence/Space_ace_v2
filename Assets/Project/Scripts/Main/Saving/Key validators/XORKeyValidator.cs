namespace SpaceAce.Main.Saving
{
    public sealed class XORKeyValidator : IKeyValidator
    {
        private const int MinKeySize = 16;

        public bool IsKeyValid(byte[] key) =>
            key is not null && key.Length >= MinKeySize;
    }
}