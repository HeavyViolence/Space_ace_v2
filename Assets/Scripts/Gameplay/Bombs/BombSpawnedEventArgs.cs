using SpaceAce.UI;

using System;

namespace SpaceAce.Gameplay.Bombs
{
    public sealed class BombSpawnedEventArgs : EventArgs
    {
        public IEntityView View { get; }

        public BombSpawnedEventArgs(IEntityView view)
        {
            View = view ?? throw new ArgumentNullException();
        }
    }
}