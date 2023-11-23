using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelStopwatchInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LevelStopwatch>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}