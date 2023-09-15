using Zenject;

namespace SpaceAce.Architecture.Installers
{
    public sealed class MasterAudioListenerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<MasterAudioListenerHolder>()
                     .FromComponentInHierarchy(false)
                     .AsSingle()
                     .NonLazy();
        }
    }
}