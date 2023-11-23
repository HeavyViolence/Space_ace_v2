using SpaceAce.Main;

using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelStopwatch : ILevelStopwatch, IInitializable, IDisposable, IPausable, ITickable
    {
        private readonly IGameStateLoader _gameStateLoader;
        private readonly ILevelCompleter _levelCompleter;
        private readonly IGamePauser _gamePauser;

        private float _timer = 0f;
        private bool _paused = true;

        public TimeSpan Stopwatch { get; private set; }

        public LevelStopwatch(IGameStateLoader gameStateLoader,
                              ILevelCompleter levelCompleter,
                              IGamePauser gamePauser)
        {
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(IGameStateLoader)}!");

            _levelCompleter = levelCompleter ?? throw new ArgumentNullException(nameof(levelCompleter),
                $"Attempted to pass an empty {typeof(ILevelCompleter)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass n empty {typeof(IGamePauser)}!");
        }

        #region interfaces

        public void Initialize()
        {
            _gamePauser.Register(this);
            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
            _levelCompleter.LevelConcluded += LevelConcludedEventHandler;
        }

        public void Dispose()
        {
            _gamePauser.Deregister(this);
            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
            _levelCompleter.LevelConcluded -= LevelConcludedEventHandler;
        }

        public void Pause()
        {
            if (_paused == true) return;
            _paused = true;
        }

        public void Resume()
        {
            if (_paused == false) return;
            _paused = false;
        }

        public void Tick()
        {
            if (_paused == true) return;

            _timer += Time.deltaTime;
            Stopwatch = TimeSpan.FromSeconds(_timer);
        }

        #endregion

        #region event handlers

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            _paused = false;
            _timer = 0f;
            Stopwatch = TimeSpan.Zero;
        }

        private void LevelConcludedEventHandler(object sender, LevelEndedEventArgs e)
        {
            _paused = true;
        }

        #endregion
    }
}