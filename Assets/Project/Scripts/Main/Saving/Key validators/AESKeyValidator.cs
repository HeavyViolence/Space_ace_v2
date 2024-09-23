using System.Collections.Generic;

namespace SpaceAce.Main.Saving
{
    public sealed class AESKeyValidator : IKeyValidator
    {
        private static readonly HashSet<int> _validKeySizes = new() { 16, 32 };

        public bool IsKeyValid(byte[] key) =>
            key is not null && _validKeySizes.Contains(key.Length) == true;
    }
}