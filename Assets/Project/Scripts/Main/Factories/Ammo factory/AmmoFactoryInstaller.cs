using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.AmmoFactories
{
    public sealed class AmmoFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private AmmoFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AmmoFactory>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}