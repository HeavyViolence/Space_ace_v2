using System;

namespace SpaceAce.Main
{
    public sealed class MainMenuLoadingStartedEventArgs : EventArgs
    {
        public float LoadingDelay { get; }

        public MainMenuLoadingStartedEventArgs(float loadingDelay)
        {
            LoadingDelay = loadingDelay;
        }
    }
}