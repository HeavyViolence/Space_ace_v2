using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.PlayerShipFactories
{
    public sealed class PlayerShipFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private PlayerShipFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerShipFactory>()
                     .AsSingle()
                     .WithArguments(_config.GetPrefabs())
                     .NonLazy();
        }
    }
}