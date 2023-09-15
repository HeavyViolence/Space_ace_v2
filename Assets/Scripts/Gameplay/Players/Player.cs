using System;

namespace SpaceAce.Gameplay.Players
{
    public sealed class Player
    {
        public event EventHandler SpaceshipSpawned, SpaceshipDefeated;
    }
}