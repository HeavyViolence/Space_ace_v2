using SpaceAce.Auxiliary;

using UnityEngine;

using Zenject;

public sealed class EasingServiceInstaller : MonoInstaller
{
    [SerializeField]
    private EasingServiceConfig _config;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<EasingService>()
                 .AsSingle()
                 .WithArguments(_config)
                 .NonLazy();
    }
}