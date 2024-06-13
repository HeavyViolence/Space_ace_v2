using SpaceAce.Main.Factories.MeteorFactories;

using UnityEngine;

using Zenject;

public sealed class MeteorFactoryInstaller : MonoInstaller
{
    [SerializeField]
    private MeteorFactoryConfig _config;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<MeteorFactory>()
                 .AsSingle()
                 .WithArguments(_config.GetPrefabs())
                 .NonLazy();
    }
}