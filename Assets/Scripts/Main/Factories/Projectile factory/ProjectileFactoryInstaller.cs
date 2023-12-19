using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class ProjectileFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private ProjectileFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ProjectileFactory>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}