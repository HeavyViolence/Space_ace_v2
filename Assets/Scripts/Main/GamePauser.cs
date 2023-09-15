using System;

namespace SpaceAce.Main
{
    public sealed class GamePauser
    {
        public event EventHandler GamePaused, GameResumed;

        public bool Paused { get; private set; } = false;

        public GamePauser() { }

        public void Pause()
        {
            if (Paused) return;

            Paused = true;
            GamePaused?.Invoke(this, EventArgs.Empty);
        }

        public void Resume()
        {
            if (Paused == false) return;

            Paused = false;
            GameResumed?.Invoke(this, EventArgs.Empty);
        }
    }
}