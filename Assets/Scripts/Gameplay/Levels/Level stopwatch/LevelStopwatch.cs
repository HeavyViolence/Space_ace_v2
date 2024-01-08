using SpaceAce.Main;

using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelStopwatch : IInitializable, IDisposable, IPausable, ITickable
    {
        private readonly GameStateLoader _gameStateLoader;
        private readonly LevelCompleter _levelCompleter;
        private readonly GamePauser _gamePauser;

        private float _timer = 0f;
        private bool _paused = true;

        public TimeSpan Stopwatch { get; private set; }

        public LevelStopwatch(GameStateLoader gameStateLoader,
                              LevelCompleter levelCompleter,
                              GamePauser gamePauser)
        {
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _levelCompleter = levelCompleter ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
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