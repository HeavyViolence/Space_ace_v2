using SpaceAce.Main;
using UnityEngine;
using Zenject;

namespace SpaceAce.Architecture.Installers
{
    public sealed class SpaceBackgroundInstaller : MonoInstaller
    {
        [SerializeField] private GameObject _spaceBackground;

        public override void InstallBindings()
        {
            Container.Bind<SpaceBackground>()
                     .FromComponentInNewPrefab(_spaceBackground)
                     .AsSingle()
                     .NonLazy();
        }
    }
}