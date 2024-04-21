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
        private bool _stopwatchPaused = false;

        private TimeSpan _stopwatch;

        public int Minutes => _stopwatch.Minutes;
        public int Seconds => _stopwatch.Seconds;
        public int Milliseconds => _stopwatch.Milliseconds;

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

        public void Tick()
        {
            if (_gameStateLoader.CurrentState != GameState.Level ||
                _gamePauser.Paused == true ||
                _stopwatchPaused == true)
            {
                return;
            }

            _timer += Time.deltaTime;
            _stopwatch = TimeSpan.FromSeconds(_timer);
        }

        public void Pause()
        {
            if (_stopwatchPaused == true) return;
            _stopwatchPaused = true;
        }

        public void Resume()
        {
            if (_stopwatchPaused == false) return;
            _stopwatchPaused = false;
        }

        #endregion

        #region event handlers

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            _timer = 0f;
            _stopwatch = TimeSpan.Zero;

            Resume();
        }

        private void LevelConcludedEventHandler(object sender, LevelEndedEventArgs e)
        {
            Pause();
        }

        #endregion
    }
}