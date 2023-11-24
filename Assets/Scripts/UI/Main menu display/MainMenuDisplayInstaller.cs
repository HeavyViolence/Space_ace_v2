using SpaceAce.Main.Audio;

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

        [SerializeField]
        private UIAudio _uiAudio;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MainMenuDisplay>()
                     .AsSingle()
                     .WithArguments(_displayAsset, _settings)
                     .NonLazy();

            Container.BindInterfacesAndSelfTo<MainMenuDisplayMediator>()
                     .AsSingle()
                     .WithArguments(_uiAudio)
                     .NonLazy();
        }
    }
}