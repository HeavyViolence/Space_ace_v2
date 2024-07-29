using System;

namespace SpaceAce.Gameplay.Enemies
{
    public sealed class EnemySpawnedEventArgs : EventArgs
    {
        public Enemy Enemy { get; }

        public EnemySpawnedEventArgs(Enemy enemy)
        {
            Enemy = enemy ?? throw new ArgumentNullException();
        }
    }
}