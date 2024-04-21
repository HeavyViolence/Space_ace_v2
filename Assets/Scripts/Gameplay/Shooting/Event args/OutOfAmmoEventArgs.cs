using System;

namespace SpaceAce.Gameplay.Shooting
{
    public sealed class OutOfAmmoEventArgs : EventArgs
    {
        public string Warning { get; }

        public OutOfAmmoEventArgs(string warning)
        {
            if (string.IsNullOrEmpty(warning) ||
                string.IsNullOrWhiteSpace(warning))
                throw new ArgumentNullException();

            Warning = warning;
        }
    }
}