using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterCameraInstaller : MonoInstaller
    {
        [SerializeField]
        private GameObject _masterCameraPrefab;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MasterCameraHolder>()
                     .AsSingle()
                     .WithArguments(_masterCameraPrefab)
                     .NonLazy();
        }
    }
}