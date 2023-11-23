using Zenject;

namespace SpaceAce.Main
{
    public sealed class GamePauserInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GamePauser>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}