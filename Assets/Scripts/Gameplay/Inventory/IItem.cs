using Cysharp.Threading.Tasks;

using System;

namespace SpaceAce.Gameplay.Inventories
{
    public interface IItem : IEquatable<IItem>
    {
        ItemSize Size { get; }
        ItemQuality Quality { get; }

        float Price { get; }

        bool Usable { get; }
        bool Tradable { get; }

        bool Use();

        UniTask<string> GetNameAsync();
        UniTask<string> GetStatsAsync();
    }
}