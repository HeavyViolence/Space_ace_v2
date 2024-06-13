using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterCameraShakerInstaller : MonoInstaller
    {
        [SerializeField]
        private MasterCameraShakerConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MasterCameraShaker>()
                     .AsSingle()
                     .WithArguments(_config.Settings, _config.ShakeCurve)
                     .NonLazy();
        }
    }
}