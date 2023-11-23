using SpaceAce.Main;
using SpaceAce.Main.Audio;

using UnityEngine;

using Zenject;

namespace SpaceAce.UI
{
    public sealed class UIMediatorInstaller : MonoInstaller
    {
        [SerializeField, Range(IGameStateLoader.MinLoadingDelay, IGameStateLoader.MaxLoadingDelay)]
        private float _loadingDelay;

        [SerializeField]
        private UIAudio _uiAudio;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UIMediator>()
                     .AsSingle()
                     .WithArguments(_loadingDelay,
                                    _uiAudio)
                     .NonLazy();
        }
    }
}