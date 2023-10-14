using SpaceAce.Architecture;
using SpaceAce.Auxiliary;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceAce.Main
{
    public sealed class SpaceBackground : IInitializable, IDisposable, IUpdatable
    {
        public const float MinScrollSpeed = 0.001f;
        public const float MaxScrollSpeed = 0.01f;

        public const float MainMenuScrollSpeed = 0.003f;
        public const float LevelScrollSpeed = 0.002f;

        private readonly Material _mainMenuSpaceBackground = null;
        private readonly List<Material> _levelsSpaceBackgrounds = null;
        private readonly ParticleSystem _dustfield = null;
        private readonly MeshRenderer _renderer = null;

        private readonly GamePauser _gamePauser = null;
        private readonly LevelsLoader _levelLoader = null;
        private readonly MainMenuLoader _mainMenuLoader = null;

        private bool _paused = false;

        private float _scrollSpeed = MinScrollSpeed;
        public float ScrollSpeed { get => _scrollSpeed; set => _scrollSpeed = Mathf.Clamp(value, MinScrollSpeed, MaxScrollSpeed); }

        public SpaceBackground(GameObject spaceBackground,
                               Material mainMenuSpaceBackground,
                               IEnumerable<Material> levelsSpaceBackgrounds,
                               GamePauser gamePauser,
                               LevelsLoader levelLoader,
                               MainMenuLoader mainMenuLoader)
        {
            if (spaceBackground == null) throw new ArgumentNullException("Attempted to pass an empty space background object!");
            if (mainMenuSpaceBackground == null) throw new ArgumentNullException("Attempted to pass an empty main menu space background!");
            if (levelsSpaceBackgrounds is null) throw new ArgumentNullException("Attempted to pass an empty levels space backgrounds!");
            if (gamePauser is null) throw new ArgumentNullException($"Attempted to pass an empty {nameof(GamePauser)}!");
            if (levelLoader is null) throw new ArgumentNullException($"Attempted to pass an empty {nameof(LevelsLoader)}!");
            if (mainMenuLoader is null) throw new ArgumentNullException($"Attempted to pass an empty {nameof(MainMenuLoader)}!");

            _mainMenuSpaceBackground = mainMenuSpaceBackground;
            _levelsSpaceBackgrounds = new List<Material>(levelsSpaceBackgrounds);
            _gamePauser = gamePauser;
            _levelLoader = levelLoader;
            _mainMenuLoader = mainMenuLoader;

            _renderer = spaceBackground.GetComponentInChildren<MeshRenderer>();
            if (_renderer == null) throw new MissingComponentException($"Space background object is missing {nameof(MeshRenderer)}!");

            _dustfield = spaceBackground.GetComponentInChildren<ParticleSystem>();
            if (_dustfield == null) throw new MissingComponentException($"Space background object is missing {nameof(ParticleSystem)}!");

            SetMainMenuState();
        }

        private void SetMainMenuState()
        {
            ScrollSpeed = MainMenuScrollSpeed;

            _renderer.sharedMaterial = _mainMenuSpaceBackground;
            _renderer.sharedMaterial.mainTextureOffset = new Vector2(0f, AuxMath.RandomNormal);

            _dustfield.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void SetLevelState()
        {
            ScrollSpeed = LevelScrollSpeed;

            int backgroundIndex = AuxMath.GetRandom(0, _levelsSpaceBackgrounds.Count);

            _renderer.sharedMaterial = _levelsSpaceBackgrounds[backgroundIndex];
            _renderer.sharedMaterial.mainTextureOffset = new Vector2(0f, AuxMath.RandomNormal);

            _dustfield.Play(true);
        }

        #region interfaces

        public void Initialize()
        {
            _gamePauser.GamePaused += GamePausedEventHandler;
            _gamePauser.GameResumed += GameResumedEventHandler;

            _mainMenuLoader.MainMenuLoaded += MainMenuLoadedEventHandler;

            _levelLoader.LevelLoaded += LevelLoadedEventHandler;
        }

        public void Dispose()
        {
            _gamePauser.GamePaused -= GamePausedEventHandler;
            _gamePauser.GameResumed -= GameResumedEventHandler;

            _mainMenuLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;

            _levelLoader.LevelLoaded -= LevelLoadedEventHandler;
        }

        void IUpdatable.Update()
        {
            if (_paused == true) return;

            _renderer.sharedMaterial.mainTextureOffset += new Vector2(0f, ScrollSpeed * Time.deltaTime);
        }

        #endregion

        #region event handlers

        private void GamePausedEventHandler(object sender, System.EventArgs e)
        {
            _paused = true;
            _dustfield.Pause(true);
        }

        private void GameResumedEventHandler(object sender, System.EventArgs e)
        {
            _paused = false;
            _dustfield.Play(true);
        }

        private void MainMenuLoadedEventHandler(object sender, System.EventArgs e)
        {
            SetMainMenuState();
        }

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            SetLevelState();
        }

        #endregion
    }
}