using UnityEngine;
using UnityEngine.UIElements;

using Zenject;

namespace SpaceAce.UI
{
    public sealed class MainMenuDisplayInstaller : MonoInstaller
    {
        [SerializeField]
        private VisualTreeAsset _displayAsset;

        [SerializeField]
        private PanelSettings _settings;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MainMenuDisplay>()
                     .AsSingle()
                     .WithArguments(_displayAsset, _settings)
                     .NonLazy();
        }
    }
}