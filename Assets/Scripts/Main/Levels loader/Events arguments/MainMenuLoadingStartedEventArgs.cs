using System;

namespace SpaceAce.Main
{
    public sealed class MainMenuLoadingStartedEventArgs : EventArgs
    {
        public float LoadingDelay { get; }

        public MainMenuLoadingStartedEventArgs(float loadingDelay)
        {
            if (loadingDelay <= 0f) throw new ArgumentOutOfRangeException(nameof(loadingDelay),
                "Loading delay must be greater than 0!");

            LoadingDelay = loadingDelay;
        }
    }
}