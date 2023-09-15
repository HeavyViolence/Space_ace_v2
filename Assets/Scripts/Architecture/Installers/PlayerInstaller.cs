using SpaceAce.Gameplay.Players;
using Zenject;

namespace SpaceAce.Architecture.Installers
{
    public sealed class PlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<Player>().FromNew().AsSingle().NonLazy();
        }
    }
}