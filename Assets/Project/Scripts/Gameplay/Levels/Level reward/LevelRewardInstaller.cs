using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelRewardInstaller : MonoInstaller
    {
        [SerializeField]
        private LevelRewardCollectorConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LevelRewardCollector>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();

            Container.BindInterfacesAndSelfTo<LevelRewardDispenser>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}