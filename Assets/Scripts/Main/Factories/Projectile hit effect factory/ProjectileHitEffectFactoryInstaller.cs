using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class ProjectileHitEffectFactoryInstaller : MonoInstaller
    {
        [SerializeField]
        private ProjectileHitEffectFactoryConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ProjectileHitEffectFactory>()
                     .AsSingle()
                     .WithArguments(_config.HitEffects)
                     .NonLazy();
        }
    }
}