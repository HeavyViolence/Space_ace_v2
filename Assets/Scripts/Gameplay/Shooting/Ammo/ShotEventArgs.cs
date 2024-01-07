using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class ShotEventArgs : EventArgs
    {
        public float HeatGenerated { get; }

        public ShotEventArgs(float heatGenerated)
        {
            HeatGenerated = Mathf.Clamp(heatGenerated, 0f, float.MaxValue);
        }
    }
}