using SpaceAce.Main;
using Zenject;

namespace SpaceAce.Architecture.Installers
{
    public sealed class SavingSystemInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<SavingSystem>().FromNew().AsSingle().NonLazy();
        }
    }
}