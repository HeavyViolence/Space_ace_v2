using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Items
{
    public sealed class ItemQualityToSpawnProbabilityConverterInstaller : MonoInstaller
    {
        [SerializeField]
        private ItemQualityToSpawnProbabilityConverterConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ItemQualityToSpawnProbabilityConverter>()
                     .AsSingle()
                     .WithArguments(_config.SpawnProbabilityCurve)
                     .NonLazy();
        }
    }
}