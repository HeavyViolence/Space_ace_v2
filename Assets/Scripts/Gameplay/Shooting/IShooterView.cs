using SpaceAce.Auxiliary;

using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Gameplay.Shooting.Guns;

using System;

namespace SpaceAce.Gameplay.Shooting
{
    public interface IShooterView
    {
        event EventHandler ShootingStarted, ShootingStopped;
        event EventHandler Overheated, CooledDown;

        event EventHandler<FloatValueChangedEventArgs> HeatValueChanged, HeatCapacityChanged;

        event EventHandler<WeaponChangedEventArgs> WeaponChanged;
        event EventHandler<AmmoChangedEventArgs> AmmoChanged;
        event EventHandler<OutOfAmmoEventArgs> OutOfAmmo;

        IGun FirstActiveGun { get; }
        AmmoSet ActiveAmmo { get; }

        float Heat { get; }
        float HeatCapacity { get; }
        float HeatNormalized => Heat / HeatCapacity;
        float HeatPercentage => HeatNormalized * 100f;
        float OverheatDuration { get; }

        bool Overheat { get; }

        float GetDamagePerSecond();
    }
}