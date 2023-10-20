using System;
using System.Collections.Generic;

namespace SpaceAce.Architecture
{
    public sealed class GamePauser
    {
        public event EventHandler GamePaused, GameResumed;

        private readonly HashSet<IPausable> _pausableEntities = new();

        public bool Paused { get; private set; } = false;

        public GamePauser() { }

        public void Register(IPausable entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity),
                $"Attempted to register an empty {typeof(IPausable)}!");

            _pausableEntities.Add(entity);
        }

        public void Deregister(IPausable entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity),
                $"Attempted to deregister an empty {typeof(IPausable)}!");

            _pausableEntities.Remove(entity);
        }

        public void Pause()
        {
            if (Paused == true) return;

            foreach (IPausable pausableEntity in _pausableEntities)
                pausableEntity.Pause();

            Paused = true;
            GamePaused?.Invoke(this, EventArgs.Empty);
        }

        public void Resume()
        {
            if (Paused == false) return;

            foreach (IPausable pausableEntity in _pausableEntities)
                pausableEntity.Resume();

            Paused = false;
            GameResumed?.Invoke(this, EventArgs.Empty);
        }
    }
}