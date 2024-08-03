using System;

namespace SpaceAce.Gameplay.Enemies
{
    public sealed class EnemySpawnedEventArgs : EventArgs
    {
        public Enemy Enemy { get; }
        public bool Boss { get; }

        public EnemySpawnedEventArgs(Enemy enemy, bool boss)
        {
            Enemy = enemy ?? throw new ArgumentNullException();
            Boss = boss;
        }
    }
}