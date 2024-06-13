using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.EnemyShipFactories
{
    public sealed class EnemyShipFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private EnemyShipFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EnemyShipFactory>()
                     .AsSingle()
                     .WithArguments(_config.GetPrefabs())
                     .NonLazy();
        }
    }
}