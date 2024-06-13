using System;

namespace SpaceAce.Gameplay.Damage
{
    public interface IDestroyable
    {
        event EventHandler<DestroyedEventArgs> Destroyed;
    }
}