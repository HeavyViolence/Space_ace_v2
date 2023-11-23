using UnityEngine;
using UnityEngine.UIElements;

using Zenject;

namespace SpaceAce.UI
{
    public sealed class GamePauseDisplayInstaller : MonoInstaller
    {
        [SerializeField]
        private VisualTreeAsset _display;

        [SerializeField]
        private PanelSettings _settings;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GamePauseDisplay>()
                     .AsSingle()
                     .WithArguments(_display, _settings)
                     .NonLazy();
        }
    }
}