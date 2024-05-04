using SpaceAce.Gameplay.Shooting.Guns;

using System;

public sealed class WeaponChangedEventArgs : EventArgs
{
    public IGun FirstActiveGun;

    public WeaponChangedEventArgs(IGun firstActiveGun)
    {
        FirstActiveGun = firstActiveGun ?? throw new ArgumentNullException();
    }
}
