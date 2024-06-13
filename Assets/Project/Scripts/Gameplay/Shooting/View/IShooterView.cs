using SpaceAce.Auxiliary;

using System;

namespace SpaceAce.Gameplay.Shooting
{
    public interface IShooterView
    {
        event EventHandler ShootingStarted, ShootingStopped;
        event EventHandler<FloatValueChangedEventArgs> HeatValueChanged, HeatCapacityChanged;
        public event EventHandler Overheated, CooledDown;
        event EventHandler GunsSwitched;

        AmmoView ActiveAmmoView { get; }

        float Heat { get; }
        float HeatCapacity { get; }
        float HeatNormalized => Heat / HeatCapacity;
        float HeatPercentage => HeatNormalized * 100f;
        float OverheatDuration { get; }

        bool Firing { get; }    
        bool GunsSelected { get; }
        bool Overheat { get; }

        float GetDamagePerSecond();
    }
}