using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class BestLevelRunStatisticsCollectorInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BestLevelRunStatisticsCollector>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}