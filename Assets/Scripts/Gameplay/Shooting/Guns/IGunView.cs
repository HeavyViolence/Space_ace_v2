using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Items;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Guns
{
    public interface IGunView
    {
        event EventHandler ShootingStarted, ShootingStopped;
        event EventHandler<ShotFiredEventArgs> ShotFired;

        Transform Transform { get; }
        Size AmmoSize { get; }

        bool FirstShotInLine { get; }
        bool IsRightHanded { get; }
        bool Firing { get; }

        float SignedConvergenceAngle { get; }
        float FireRate { get; }
        float Dispersion { get; }

        UniTask<string> GetSizeCodeAsync();
    }
}