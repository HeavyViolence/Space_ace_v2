using System;

namespace SpaceAce.Main
{
    public sealed class MainMenuLoadingStartedEventArgs : EventArgs
    {
        public float LoadingDelay { get; }

        public MainMenuLoadingStartedEventArgs(float LoadingDelay)
        {
            this.LoadingDelay = LoadingDelay;
        }
    }
}