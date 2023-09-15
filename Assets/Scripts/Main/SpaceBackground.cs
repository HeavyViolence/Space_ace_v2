using SpaceAce.Auxiliary;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SpaceAce.Main
{
    public sealed class SpaceBackground : MonoBehaviour
    {
        public const float MinScrollSpeed = 0.001f;
        public const float MaxScrollSpeed = 0.01f;

        public const float MainMenuScrollSpeed = 0.003f;
        public const float LevelScrollSpeed = 0.002f;

        [SerializeField] private Material _mainMenuSpaceBackground;
        [SerializeField] private List<Material> _spaceBackgrounds;
        [SerializeField] private ParticleSystem _dustfield;
        [SerializeField] private MeshRenderer _renderer;

        private GamePauser _gamePauser = null;
        private LevelLoader _levelLoader = null;

        private float _scrollSpeed = MinScrollSpeed;
        public float ScrollSpeed { get => _scrollSpeed; set => _scrollSpeed = Mathf.Clamp(value, MinScrollSpeed, MaxScrollSpeed); }

        public bool ScrollingEnabled { get; private set; } = true;

        [Inject]
        private void Construct(GamePauser gamePauser, LevelLoader levelLoader)
        {
            _gamePauser = gamePauser;
            _levelLoader = levelLoader;
        }

        private void OnEnable()
        {
            _gamePauser.GamePaused += GamePausedEventHandler;
            _gamePauser.GameResumed += GameResumedEventHandler;

            _levelLoader.MainMenuLoaded += MainMenuLoadedEventHandler;
            _levelLoader.LevelLoaded += LevelLoadedEventHandler;
        }

        private void OnDisable()
        {
            _gamePauser.GamePaused -= GamePausedEventHandler;
            _gamePauser.GameResumed -= GameResumedEventHandler;

            _levelLoader.MainMenuLoaded -= MainMenuLoadedEventHandler;
            _levelLoader.LevelLoaded -= LevelLoadedEventHandler;
        }

        private void Start ()
        {
            SetMainMenuState();
        }

        private void Update()
        {
            if (ScrollingEnabled == false) return;

            _renderer.sharedMaterial.mainTextureOffset += new Vector2(0f, ScrollSpeed * Time.deltaTime);
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

            int backgroundIndex = AuxMath.GetRandom(0, _spaceBackgrounds.Count);

            _renderer.sharedMaterial = _spaceBackgrounds[backgroundIndex];
            _renderer.sharedMaterial.mainTextureOffset = new Vector2(0f, AuxMath.RandomNormal);

            _dustfield.Play(true);
        }

        #region event handlers

        private void GamePausedEventHandler(object sender, System.EventArgs e)
        {
            ScrollingEnabled = false;
            _dustfield.Pause(true);
        }

        private void GameResumedEventHandler(object sender, System.EventArgs e)
        {
            ScrollingEnabled = true;
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