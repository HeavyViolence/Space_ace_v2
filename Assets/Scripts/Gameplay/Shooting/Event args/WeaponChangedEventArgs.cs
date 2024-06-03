using SpaceAce.Gameplay.Shooting.Guns;

using System;

public sealed class WeaponChangedEventArgs : EventArgs
{
    public IGunView FirstActiveGun;

    public WeaponChangedEventArgs(IGunView firstActiveGun)
    {
        FirstActiveGun = firstActiveGun ?? throw new ArgumentNullException();
    }
}
