using System;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class ShotFiredEventArgs : EventArgs
    {
        public float Heat { get; }

        public ShotFiredEventArgs(float heat)
        {
            Heat = heat;
        }
    }
}