using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Audio
{
    public sealed class MusicPlayerInstaller : MonoInstaller
    {
        [SerializeField]
        private AudioCollection _music;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MusicPlayer>()
                     .AsSingle()
                     .WithArguments(_music)
                     .NonLazy();
        }
    }
}