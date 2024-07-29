using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Enemies
{
    public sealed class EnemySpawnerInstaller : MonoInstaller
    {
        [SerializeField]
        private EnemySpawnerConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EnemySpawner>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}