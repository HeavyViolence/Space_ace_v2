using Cysharp.Threading.Tasks;

using System.Threading;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public interface INaniteTarget
    {
        UniTask<bool> TryApplyNanitesAsync(Nanites nanites, CancellationToken token = default);
    }
}