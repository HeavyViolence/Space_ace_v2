using UnityEngine;
using UnityEngine.Audio;

using Zenject;

namespace SpaceAce.Main.Audio
{
    public sealed class AudioPlayerInstaller : MonoInstaller
    {
        [SerializeField]
        private AudioMixer _audioMixer;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AudioPlayer>()
                     .AsSingle()
                     .WithArguments(_audioMixer)
                     .NonLazy();
        }
    }
}