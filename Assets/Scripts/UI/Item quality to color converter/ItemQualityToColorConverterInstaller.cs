using SpaceAce.UI;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class ItemQualityToColorConverterInstaller : MonoInstaller
    {
        [SerializeField]
        private ItemQualityToColorConverterConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ItemQualityToColorConverter>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}