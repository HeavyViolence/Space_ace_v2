using Cysharp.Threading.Tasks;

using System;
using System.Threading;

namespace SpaceAce.Gameplay.Items
{
    public interface IItem : IEquatable<IItem>
    {
        event EventHandler Depleted;

        Size Size { get; }
        Quality Quality { get; }

        float Price { get; }

        bool Usable { get; }
        bool Tradable { get; }

        UniTask<bool> TryUseAsync(object user, CancellationToken token = default);

        UniTask<string> GetNameAsync();
        UniTask<string> GetDescriptionAsync();

        ItemSavableState GetSavableState();
    }
}