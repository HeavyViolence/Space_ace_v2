using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterCameraShakerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MasterCameraShaker>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}