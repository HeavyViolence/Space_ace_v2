using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class ShipFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private ShipFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ShipFactory>()
                     .AsSingle()
                     .WithArguments(_config.Content)
                     .NonLazy();
        }
    }
}