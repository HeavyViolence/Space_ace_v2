using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Items
{
    public sealed class ItemPropertyEvaluatorInstaller : MonoInstaller
    {
        [SerializeField]
        private ItemPropertyEvaluatorConfig _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ItemPropertyEvaluator>()
                     .AsSingle()
                     .WithArguments(_config)
                     .NonLazy();
        }
    }
}