using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class HitFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private HitFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HitFactory>()
                     .AsSingle()
                     .WithArguments(_config.HitEffects)
                     .NonLazy();
        }
    }
}