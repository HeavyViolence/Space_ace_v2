using SpaceAce.Gameplay.Shooting.Ammo;

using System;

namespace SpaceAce.Gameplay.Shooting.Guns
{
    public sealed class ShotFiredEventArgs : EventArgs
    {
        public ShotResult ShotResult { get; }

        public ShotFiredEventArgs(ShotResult result)
        {
            ShotResult = result;
        }
    }
}