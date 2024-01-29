using System;
using System.Threading;

using Cysharp.Threading.Tasks;

namespace SpaceAce.Gameplay.Movement
{
    public interface IEscapable
    {
        event EventHandler Escaped;

        UniTask WaitForEscapeAsync(float delay = 0f, CancellationToken token = default);
    }
}