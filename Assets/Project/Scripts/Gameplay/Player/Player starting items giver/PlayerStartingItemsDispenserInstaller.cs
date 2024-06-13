using SpaceAce.Gameplay.Players;

using UnityEngine;

using Zenject;

public sealed class PlayerStartingItemsDispenserInstaller : MonoInstaller
{
    [SerializeField]
    private PlayerStartingItemsDispenserConfig _config;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<PlayerStartingItemsDispenser>()
                 .AsSingle()
                 .WithArguments(_config)
                 .NonLazy();
    }
}