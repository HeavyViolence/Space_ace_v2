using Cysharp.Threading.Tasks;

using System.Threading;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public interface IEMPTarget
    {
        bool EMPActive { get; }
        float EMPFactor { get; }

        UniTask<bool> TryApplyEMPAsync(EMP emp, CancellationToken token = default);
    }
}