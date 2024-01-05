using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class ShotEventArgs : EventArgs
    {
        public int AmmoUsed { get; }
        public float HeatGenerated { get; }

        public ShotEventArgs(int ammoUsed, float heatGenerated)
        {
            AmmoUsed = Mathf.Clamp(ammoUsed, 0, int.MaxValue);
            HeatGenerated = Mathf.Clamp(heatGenerated, 0f, float.MaxValue);
        }
    }
}