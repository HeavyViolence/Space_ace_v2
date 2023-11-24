using SpaceAce.Main.Audio;

using System;

using Zenject;

namespace SpaceAce.UI
{
    public abstract class UIDisplayMediator : IInitializable, IDisposable
    {
        protected readonly AudioPlayer AudioPlayer;
        protected readonly UIAudio UIAudio;

        public UIDisplayMediator(AudioPlayer audioPlayer,
                                 UIAudio uIAudio)
        {
            AudioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer),
                $"Attempted to pass an empty {typeof(AudioPlayer)}!");

            if (uIAudio == null)
                throw new ArgumentNullException(nameof(uIAudio),
                    $"Attempted to pass an empty {typeof(UIAudio)}!");

            UIAudio = uIAudio;
        }

        public abstract void Initialize();
        public abstract void Dispose();
    }
}