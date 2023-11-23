using UnityEngine;

using Zenject;

namespace SpaceAce.UI
{
    public sealed class LevelSelectionDisplayInstaller : MonoInstaller
    {
        [SerializeField]
        private LevelSelectionDisplayAssets _levelSelectionDisplayAssets;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LevelSelectionDisplay>()
                     .AsSingle()
                     .WithArguments(_levelSelectionDisplayAssets)
                     .NonLazy();
        }
    }
}