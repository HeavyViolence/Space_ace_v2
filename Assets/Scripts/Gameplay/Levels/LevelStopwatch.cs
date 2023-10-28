using SpaceAce.Architecture;
using SpaceAce.Main;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class LevelStopwatch : IInitializable, IDisposable, IPausable, IUpdatable
    {
        private readonly GameStateLoader _gameStateLoader = null;
        private readonly LevelsCompleter _levelsCompleter = null;
        private readonly GamePauser _gamePauser = null;

        private float _timer = 0f;
        private bool _paused = true;

        public TimeSpan Stopwatch { get; private set; }

        public LevelStopwatch(GameStateLoader gameStateLoader, LevelsCompleter levelsCompleter, GamePauser gamePauser)
        {
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

            _levelsCompleter = levelsCompleter ?? throw new ArgumentNullException(nameof(levelsCompleter),
                    $"Attempted to pass an empty {typeof(LevelsCompleter)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                    $"Attempted to pass an empty {typeof(GamePauser)}!");
        }

        #region interfaces

        public void Initialize()
        {
            _gamePauser.Register(this);
            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
            _levelsCompleter.LevelConcluded += LevelConcludedEventHandler;
        }

        public void Dispose()
        {
            _gamePauser.Deregister(this);
            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
            _levelsCompleter.LevelConcluded -= LevelConcludedEventHandler;
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

        public void Update()
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