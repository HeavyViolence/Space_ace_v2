using SpaceAce.Main;
using UnityEngine;
using Zenject;

namespace SpaceAce.Architecture.Installers
{
    public sealed class MasterCameraHolderInstaller : MonoInstaller
    {
        [SerializeField] private GameObject _masterCameraHolder;

        public override void InstallBindings()
        {
            Container.Bind<MasterCameraHolder>()
                     .FromComponentInNewPrefab(_masterCameraHolder)
                     .AsSingle()
                     .NonLazy();
        }
    }
}