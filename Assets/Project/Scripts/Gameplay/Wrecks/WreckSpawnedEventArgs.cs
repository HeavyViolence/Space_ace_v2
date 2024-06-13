using SpaceAce.UI;

using System;

namespace SpaceAce.Gameplay.Wrecks
{
    public sealed class WreckSpawnedEventArgs : EventArgs
    {
        public IEntityView View { get; }

        public WreckSpawnedEventArgs(IEntityView view)
        {
            View = view ?? throw new ArgumentNullException();
        }
    }
}