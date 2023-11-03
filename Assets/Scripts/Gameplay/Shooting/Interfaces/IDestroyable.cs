using System;

namespace SpaceAce.Gameplay.Shooting
{
    public interface IDestroyable
    {
        event EventHandler<DestroyedEventArgs> Destroyed;
    }
}