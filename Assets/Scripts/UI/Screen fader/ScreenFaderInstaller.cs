using UnityEngine;
using UnityEngine.UIElements;

using Zenject;

namespace SpaceAce.UI
{
    public sealed class ScreenFaderInstaller : MonoInstaller
    {
        [SerializeField]
        private VisualTreeAsset _dispaly;

        [SerializeField]
        private PanelSettings _settings;

        [SerializeField]
        private AnimationCurve _fadingCurve;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ScreenFader>()
                     .AsSingle()
                     .WithArguments(_dispaly, _settings, _fadingCurve)
                     .NonLazy();
        }
    }
}