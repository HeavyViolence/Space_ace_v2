using SpaceAce.Main;
using Zenject;

namespace SpaceAce.Architecture.Installers
{
    public sealed class GamePauserInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GamePauser>().FromNew().AsSingle().NonLazy();
        }
    }
}
