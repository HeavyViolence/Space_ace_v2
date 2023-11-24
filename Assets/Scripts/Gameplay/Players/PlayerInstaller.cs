using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Players
{
    public sealed class PlayerInstaller : MonoInstaller
    {
        [SerializeField]
        private Vector3 _shipSpawnPosition;

        [SerializeField, Range(0f, 10000f)]
        private float _initialWalletBalance;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Player>()
                     .AsSingle()
                     .WithArguments(_shipSpawnPosition,
                                    _initialWalletBalance)
                     .NonLazy();
        }
    }
}