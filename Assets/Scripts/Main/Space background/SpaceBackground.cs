using SpaceAce.Auxiliary;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class SpaceBackground : IInitializable, IDisposable, ITickable
    {
        public const float MinScrollSpeed = 0.001f;
        public const float MaxScrollSpeed = 0.01f;

        public const float MainMenuScrollSpeed = 0.003f;
        public const float LevelScrollSpeed = 0.002f;

        private readonly Material _mainMenuSpaceBackground;
        private readonly List<Material> _levelSpaceBackgrounds;
        private readonly ParticleSystem _dustfield;
        private readonly MeshRenderer _renderer;
        private readonly GamePauser _gamePauser;
        private readonly GameStateLoader _gameStateLoader;

        private bool _paused = false;

        private float _scrollSpeed = MinScrollSpeed;
        public float ScrollSpeed { get => _scrollSpeed; set => _scrollSpeed = Mathf.Clamp(value, MinScrollSpeed, MaxScrollSpeed); }

        public SpaceBackground(GameObject spaceBackgroundPrefab,
                               Material mainMenuSpaceBackground,
                               IEnumerable<Material> levelSpaceBackgrounds,
                               GamePauser gamePauser,
                               GameStateLoader gameStateLoader)
        {
            if (spaceBackgroundPrefab == null)
                throw new ArgumentNullException("Attempted to pass an empty space background prefab!");

            if (mainMenuSpaceBackground == null)
                throw new ArgumentNullException("Attempted to pass an empty main menu space background!");

            if (levelSpaceBackgrounds is null)
                throw new ArgumentNullException("Attempted to pass an empty levels space backgrounds!");

            _mainMenuSpaceBackground = mainMenuSpaceBackground;
            _levelSpaceBackgrounds = new List<Material>(levelSpaceBackgrounds);

            GameObject spaceBackground = UnityEngine.Object.Instantiate(spaceBackgroundPrefab, Vector3.zero, Quaternion.identity);

            _renderer = spaceBackground.GetComponentInChildren<MeshRenderer>();

            if (_renderer == null)
                throw new MissingComponentException($"Space background object is missing {nameof(MeshRenderer)}!");

            _dustfield = spaceBackground.GetComponentInChildren<ParticleSystem>();

            if (_dustfield == null)
                throw new MissingComponentException($"Space background object is missing {nameof(ParticleSystem)}!");

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");

            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException(nameof(gameStateLoader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");

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

            int backgroundIndex = AuxMath.GetRandom(0, _levelSpaceBackgrounds.Count);

            _renderer.sharedMaterial = _levelSpaceBackgrounds[backgroundIndex];
            _renderer.sharedMaterial.mainTextureOffset = new Vector2(0f, AuxMath.RandomNormal);

            _dustfield.Play(true);
        }

        #region interfaces

        public void Initialize()
        {
            _gamePauser.GamePaused += GamePausedEventHandler;
            _gamePauser.GameResumed += GameResumedEventHandler;

            _gameStateLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
        }

        public void Dispose()
        {
            _gamePauser.GamePaused -= GamePausedEventHandler;
            _gamePauser.GameResumed -= GameResumedEventHandler;

            _gameStateLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
        }

        public void Tick()
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