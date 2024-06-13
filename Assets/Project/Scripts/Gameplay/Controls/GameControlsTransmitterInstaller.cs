using Zenject;

namespace SpaceAce.Gameplay.Controls
{
    public sealed class GameControlsTransmitterInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameControlsTransmitter>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}