using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Items;

using UnityEngine;


namespace SpaceAce.Gameplay.Shooting.Guns
{
    public interface IGun
    {
        Transform Transform { get; }
        Size AmmoSize { get; }
        bool IsRightHanded { get; }
        float SignedConvergenceAngle { get; }
        float FireRate { get; }
        float Dispersion { get; }

        UniTask<string> GetSizeCodeAsync();
    }
}