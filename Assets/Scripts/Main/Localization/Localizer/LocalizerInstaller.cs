using UnityEngine;
using UnityEngine.Localization;

using Zenject;

namespace SpaceAce.Main.Localization
{
    public sealed class LocalizerInstaller : MonoInstaller
    {
        [SerializeField]
        private LocalizedFont _localizedFont;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Localizer>()
                     .AsSingle()
                     .WithArguments(_localizedFont)
                     .NonLazy();
        }
    }
}