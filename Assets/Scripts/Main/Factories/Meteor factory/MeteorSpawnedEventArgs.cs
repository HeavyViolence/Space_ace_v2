using SpaceAce.UI;

using System;

namespace SpaceAce.Main.Factories.MeteorFactories
{
    public sealed class MeteorSpawnedEventArgs : EventArgs
    {
        public IEntityView View { get; }

        public MeteorSpawnedEventArgs(IEntityView view)
        {
            View = view ?? throw new ArgumentNullException();
        }
    }
}