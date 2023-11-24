using SpaceAce.Main.Audio;

using UnityEngine;

using Zenject;

namespace SpaceAce.UI
{
    public sealed class LevelSelectionDisplayInstaller : MonoInstaller
    {
        [SerializeField]
        private LevelSelectionDisplayAssets _levelSelectionDisplayAssets;

        [SerializeField]
        private UISettings _settings;

        [SerializeField]
        private UIAudio _uiAudio;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LevelSelectionDisplay>()
                     .AsSingle()
                     .WithArguments(_levelSelectionDisplayAssets)
                     .NonLazy();

            Container.BindInterfacesAndSelfTo<LevelSelectionDisplayMediator>()
                     .AsSingle()
                     .WithArguments(_settings, _uiAudio)
                     .NonLazy();
        }
    }
}