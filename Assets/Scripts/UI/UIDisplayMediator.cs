using SpaceAce.Main.Audio;

using System;

using Zenject;

namespace SpaceAce.UI
{
    public abstract class UIDisplayMediator : IInitializable, IDisposable
    {
        protected readonly AudioPlayer AudioPlayer;
        protected readonly UIAudio UIAudio;

        public UIDisplayMediator(AudioPlayer audioPlayer, UIAudio uiAudio)
        {
            AudioPlayer = audioPlayer ?? throw new ArgumentNullException();

            if (uiAudio == null) throw new ArgumentNullException();
            UIAudio = uiAudio;
        }

        public abstract void Initialize();
        public abstract void Dispose();
    }
}