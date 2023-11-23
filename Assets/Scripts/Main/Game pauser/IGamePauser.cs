using System;

namespace SpaceAce.Main
{
    public interface IGamePauser
    {
        event EventHandler GamePaused, GameResumed;

        bool Paused { get; }

        void Register(IPausable entity);
        void Deregister(IPausable entity);

        void Pause();
        void Resume();
    }
}