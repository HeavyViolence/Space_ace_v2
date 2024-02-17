using Cysharp.Threading.Tasks;

using System.Threading;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public interface IEMPTarget
    {
        UniTask<bool> TryApplyEMPAsync(EMP emp, CancellationToken token = default);
    }
}