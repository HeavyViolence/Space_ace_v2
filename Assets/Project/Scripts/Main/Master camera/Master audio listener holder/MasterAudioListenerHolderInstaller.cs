using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterAudioListenerHolderInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MasterAudioListenerHolder>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}