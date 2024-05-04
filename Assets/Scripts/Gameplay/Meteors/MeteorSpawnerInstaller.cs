using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Meteors
{
    public sealed class MeteorSpawnerInstaller : MonoInstaller
    {
        [SerializeField]
        private MeteorSpawnerConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MeteorSpawner>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}