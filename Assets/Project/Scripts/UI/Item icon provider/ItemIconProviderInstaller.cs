using UnityEngine;

using Zenject;

namespace SpaceAce.UI
{
    public sealed class ItemIconProviderInstaller : MonoInstaller
    {
        [SerializeField]
        private ItemIconProviderConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ItemIconProvider>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}