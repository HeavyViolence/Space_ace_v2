using Cysharp.Threading.Tasks;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Items
{
    public interface IItem : IEquatable<IItem>, IDisposable
    {
        event EventHandler Depleted;

        Sprite Icon { get; }

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