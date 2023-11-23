using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelUnlockerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LevelUnlocker>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}