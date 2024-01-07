using Cysharp.Threading.Tasks;

using System;
using System.Threading;

namespace SpaceAce.Gameplay.Inventories
{
    public interface IItem : IEquatable<IItem>
    {
        ItemSize Size { get; }
        ItemQuality Quality { get; }

        float Price { get; }

        bool Usable { get; }
        bool Tradable { get; }

        UniTask<bool> UseAsync(ItemStack holder,
                               CancellationToken token = default,
                               params object[] args);

        UniTask<string> GetNameAsync();
        UniTask<string> GetDescriptionAsync();
    }
}