using SpaceAce.Gameplay.Shooting.Guns;

using System;

public sealed class WeaponChangedEventArgs : EventArgs
{
    public Gun FirstActiveGun;

    public WeaponChangedEventArgs(Gun firstActiveGun)
    {
        if (firstActiveGun == null) throw new ArgumentNullException();

        FirstActiveGun = firstActiveGun;
    }
}
