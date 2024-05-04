using Zenject;

namespace SpaceAce.Main.Factories.SavedItemsFactories
{
    public sealed class SavedItemsFactoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SavedItemsFactory>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}