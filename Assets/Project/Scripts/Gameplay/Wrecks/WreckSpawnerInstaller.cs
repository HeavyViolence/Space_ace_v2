using SpaceAce.Gameplay.Wrecks;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Wrecks
{
    public sealed class WreckSpawnerInstaller : MonoInstaller
    {
        [SerializeField]
        private WreckSpawnerConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<WreckSpawner>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}