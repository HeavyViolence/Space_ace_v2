using SpaceAce.Main.Audio;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelCompleterInstaller : MonoInstaller
    {
        [SerializeField]
        private AudioCollection _levelCompletedAudio;

        [SerializeField]
        private AudioCollection _levelFailedAudio;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LevelCompleter>()
                     .AsSingle()
                     .WithArguments(_levelCompletedAudio, _levelFailedAudio)
                     .NonLazy();
        }
    }
}