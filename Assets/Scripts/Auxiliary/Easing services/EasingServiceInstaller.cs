using UnityEngine;

using Zenject;

namespace SpaceAce.Auxiliary.Easing
{
    public sealed class EasingServiceInstaller : MonoInstaller
    {
        [SerializeField]
        private EasingServiceConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EasingService>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}