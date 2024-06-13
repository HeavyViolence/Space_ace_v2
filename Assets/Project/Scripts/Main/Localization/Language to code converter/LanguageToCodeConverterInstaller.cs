using Zenject;

namespace SpaceAce.Main.Localization
{
    public sealed class LanguageToCodeConverterInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LanguageToCodeConverter>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}