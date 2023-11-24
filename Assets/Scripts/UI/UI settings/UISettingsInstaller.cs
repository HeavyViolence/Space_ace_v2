using UnityEngine;

using Zenject;

namespace SpaceAce.UI
{
    public sealed class UISettingsInstaller : MonoInstaller
    {
        [SerializeField]
        private UISettings _setings;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UISettings>()
                     .FromInstance(_setings)
                     .AsSingle()
                     .NonLazy();
        }
    }
}