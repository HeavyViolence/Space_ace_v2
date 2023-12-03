using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterCameraShakerInstaller : MonoInstaller
    {
        [SerializeField]
        private MasterCameraShakerSettings _settings;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MasterCameraShaker>()
                     .AsSingle()
                     .WithArguments(_settings)
                     .NonLazy();
        }
    }
}