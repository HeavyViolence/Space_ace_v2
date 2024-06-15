using Cysharp.Threading.Tasks;

using System.Threading;

namespace SpaceAce.Gameplay.Effects
{
    public interface INaniteTarget
    {
        bool NanitesActive { get; }

        UniTask<bool> TryApplyNanitesAsync(Nanites nanites, CancellationToken token = default);
    }
}