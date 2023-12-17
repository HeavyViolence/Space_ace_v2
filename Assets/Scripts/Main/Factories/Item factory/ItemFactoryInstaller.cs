using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class ItemFactoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ItemFactory>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}