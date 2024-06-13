using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Players
{
    public sealed class PlayerInstaller : MonoInstaller
    {
        [SerializeField]
        private Vector3 _shipSpawnPosition;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Player>()
                     .AsSingle()
                     .WithArguments(_shipSpawnPosition)
                     .NonLazy();
        }
    }
}