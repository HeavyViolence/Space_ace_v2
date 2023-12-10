using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class ExplosionFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private ExplosionFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ExplosionFactory>()
                     .AsSingle()
                     .WithArguments(_config.Explosions)
                     .NonLazy();
        }
    }
}