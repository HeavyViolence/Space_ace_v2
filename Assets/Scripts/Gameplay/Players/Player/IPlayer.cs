using System;

namespace SpaceAce.Gameplay.Players
{
    public interface IPlayer
    {
        event EventHandler SpaceshipSpawned;
        event EventHandler SpaceshipDefeated;

        IWallet Wallet { get; }
        IExperience Experience { get; }
    }
}