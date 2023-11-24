using SpaceAce.Main.Audio;

using UnityEngine;
using UnityEngine.UIElements;

using Zenject;

namespace SpaceAce.UI
{
    public sealed class LevelDisplayInstaller : MonoInstaller
    {
        [SerializeField]
        private VisualTreeAsset _display;

        [SerializeField]
        private PanelSettings _settings;

        [SerializeField]
        private UIAudio _uiAudio;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LevelDisplay>()
                     .AsSingle()
                     .WithArguments(_display, _settings)
                     .NonLazy();

            Container.BindInterfacesAndSelfTo<LevelDisplayMediator>()
                     .AsSingle()
                     .WithArguments(_uiAudio)
                     .NonLazy();
        }
    }
}