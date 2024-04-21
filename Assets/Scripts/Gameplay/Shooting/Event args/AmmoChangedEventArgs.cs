using SpaceAce.Gameplay.Shooting.Ammo;

using System;

namespace SpaceAce.Gameplay.Shooting
{
    public sealed class AmmoChangedEventArgs : EventArgs
    {
        public AmmoSet PreviousAmmo { get; }
        public AmmoSet ActiveAmmo { get; }

        public AmmoChangedEventArgs(AmmoSet previousAmmo, AmmoSet activeAmmo)
        {
            PreviousAmmo = previousAmmo ?? throw new ArgumentNullException();
            ActiveAmmo = activeAmmo ?? throw new ArgumentNullException();
        }
    }
}