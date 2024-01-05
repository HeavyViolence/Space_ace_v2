using SpaceAce.Gameplay.Shooting.Ammo;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class AmmoFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private AmmoConfigs _ammoConfigs;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AmmoFactory>()
                     .AsSingle()
                     .WithArguments(_ammoConfigs)
                     .NonLazy();
        }
    }
}