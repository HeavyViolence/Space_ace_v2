using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.BombFactories
{
    public sealed class BombFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private BombFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BombFactory>()
                     .AsSingle()
                     .WithArguments(_config.GetPrefabs())
                     .NonLazy();
        }
    }
}