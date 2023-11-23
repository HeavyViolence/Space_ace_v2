using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public class SpaceBackgroundInstaller : MonoInstaller
    {
        [SerializeField]
        private GameObject _spaceBackgroundPrefab;

        [SerializeField]
        private Material _mainMenuSpaceBackground;

        [SerializeField]
        private List<Material> _levelSpaceBackground;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SpaceBackground>()
                     .AsSingle()
                     .WithArguments(_spaceBackgroundPrefab, _mainMenuSpaceBackground, _levelSpaceBackground)
                     .NonLazy();
        }
    }
}