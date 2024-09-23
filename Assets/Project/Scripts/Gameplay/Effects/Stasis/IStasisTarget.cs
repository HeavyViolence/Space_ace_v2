using Cysharp.Threading.Tasks;

using System.Threading;

namespace SpaceAce.Gameplay.Effects
{
    public interface IStasisTarget
    {
        bool StasisActive { get; }
        float SpeedFactor { get; }

        UniTask<bool> TryApplyStasisAsync(Stasis stasis, CancellationToken token = default);
    }
}