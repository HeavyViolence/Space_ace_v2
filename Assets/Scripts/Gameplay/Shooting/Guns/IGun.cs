using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Shooting.Ammo;

using System.Threading;

namespace SpaceAce.Gameplay.Shooting.Guns
{
    public interface IGun
    {
        UniTask FireAsync(object shooter,
                          AmmoSet ammo,
                          CancellationToken fireCancellation = default,
                          CancellationToken overheatCancellation = default);
    }
}