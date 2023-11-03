using System;
using System.Threading;

using Cysharp.Threading.Tasks;

namespace SpaceAce.Gameplay.Movement
{
    public interface IEscapable
    {
        event EventHandler Escaped;

        UniTask WatchForEscapeAsync(Func<bool> condition, CancellationToken token);
    }
}