using SpaceAce.Main;
using Zenject;

namespace SpaceAce.Architecture.Installers
{
    public sealed class CameraShakerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<CameraShaker>()
                     .FromComponentInHierarchy(false)
                     .AsSingle()
                     .NonLazy();
        }
    }
}