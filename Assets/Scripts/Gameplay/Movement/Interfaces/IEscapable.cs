using System;
using System.Threading;

using Cysharp.Threading.Tasks;

namespace SpaceAce.Gameplay.Movement
{
    public interface IEscapable
    {
        event EventHandler Escaped;

        UniTask WaitForEscapeAsync(Func<bool> condition, float delay, CancellationToken token = default);
    }
}