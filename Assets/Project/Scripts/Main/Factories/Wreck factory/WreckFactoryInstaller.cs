using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.WreckFactories
{
    public sealed class WreckFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private WreckFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<WreckFactory>()
                     .AsSingle()
                     .WithArguments(_config.GetPrefabs())
                     .NonLazy();
        }
    }
}