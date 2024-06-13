using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Bombs
{
    public sealed class BombSpawnerInstaller : MonoInstaller
    {
        [SerializeField]
        private BombSpawnerConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BombSpawner>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}