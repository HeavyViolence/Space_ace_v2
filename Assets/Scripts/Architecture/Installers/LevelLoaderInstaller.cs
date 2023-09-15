using SpaceAce.Main;
using Zenject;

namespace SpaceAce.Architecture.Installers
{
    public sealed class LevelLoaderInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<LevelLoader>().FromNew().AsSingle().NonLazy();
        }
    }
}