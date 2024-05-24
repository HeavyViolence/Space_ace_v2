using System;

namespace SpaceAce.Main
{
    public sealed class MainMenuLoadingStartedEventArgs : EventArgs
    {
        public float LoadingDelay { get; }

        public MainMenuLoadingStartedEventArgs(float loadingDelay)
        {
            if (loadingDelay <= 0f) throw new ArgumentOutOfRangeException();
            LoadingDelay = loadingDelay;
        }
    }
}