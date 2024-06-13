using SpaceAce.Main.Audio;

using UnityEngine;
using UnityEngine.UIElements;

using Zenject;

namespace SpaceAce.UI.Displays
{
    public sealed class GamePauseDisplayInstaller : MonoInstaller
    {
        [SerializeField]
        private VisualTreeAsset _display;

        [SerializeField]
        private PanelSettings _settings;

        [SerializeField]
        private UISettings _uiSettings;

        [SerializeField]
        private UIAudio _uiAudio;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GamePauseDisplay>()
                     .AsSingle()
                     .WithArguments(_display, _settings)
                     .NonLazy();

            Container.BindInterfacesAndSelfTo<GamePauseDisplayMediator>()
                     .AsSingle()
                     .WithArguments(_uiSettings, _uiAudio)
                     .NonLazy();
        }
    }
}