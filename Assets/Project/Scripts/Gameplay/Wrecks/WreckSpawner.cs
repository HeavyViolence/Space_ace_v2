using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories.WreckFactories;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Wrecks
{
    public sealed class WreckSpawner : IInitializable, IDisposable
    {
        private const float SpawnPositionOffsetFactor = 1.3f;
        private const float EscapeDelay = 3f;

        public event EventHandler SpawnStarted, SpawnEnded;
        public event EventHandler WaveStarted, WaveEnded;
        public event EventHandler ShowerStarted, ShowerEnded;
        public event EventHandler<WreckSpawnedEventArgs> WreckSpawned;

        private readonly WreckSpawnerConfig _config;
        private readonly WreckFactory _wreckFactory;
        private readonly GameStateLoader _gameStateLoader;
        private readonly LevelCompleter _levelCompleter;
        private readonly GamePauser _gamePauser;
        private readonly MasterCameraHolder _masterCameraHolder;
        private readonly AudioPlayer _audioPlayer;

        private CancellationTokenSource _spawnCancellation;

        public bool SpawnActive { get; private set; } = false;

        private MovementBehaviour WreckMovement => delegate (Rigidbody2D body, MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        public WreckSpawner(WreckSpawnerConfig config,
                            WreckFactory wreckFactory,
                            GameStateLoader gameStateLoader,
                            LevelCompleter levelCompleter,
                            GamePauser gamePauser,
                            MasterCameraHolder masterCameraHolder,
                            AudioPlayer audioPlayer)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;

            _wreckFactory = wreckFactory ?? throw new ArgumentNullException();
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _levelCompleter = levelCompleter ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _masterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException();
        }

        #region interfaces

        public void Initialize()
        {
            _gameStateLoader.LevelLoaded += LevelLoadedEventHandler;
            _levelCompleter.LevelConcluded += LevelConcludedEventHandler;
        }

        public void Dispose()
        {
            _gameStateLoader.LevelLoaded -= LevelLoadedEventHandler;
            _levelCompleter.LevelConcluded -= LevelConcludedEventHandler;
        }

        #endregion

        #region event handlers

        private void LevelLoadedEventHandler(object sender, LevelLoadedEventArgs e)
        {
            _spawnCancellation = new();
            SpawnAsync(e.Level, _spawnCancellation.Token).Forget();
        }

        private void LevelConcludedEventHandler(object sender, LevelEndedEventArgs e)
        {
            _spawnCancellation?.Cancel();
            _spawnCancellation?.Dispose();
            _spawnCancellation = null;
        }

        private async UniTask SpawnAsync(int level, CancellationToken token)
        {
            SpawnStarted?.Invoke(this, EventArgs.Empty);
            SpawnActive = true;

            while (token.IsCancellationRequested == false)
            {
                WreckWave wave = _config.NextWave(level);

                if (wave.WreckShower == true) ShowerStarted?.Invoke(this, EventArgs.Empty);
                else WaveStarted?.Invoke(this, EventArgs.Empty);

                foreach (WreckWaveSlot slot in wave)
                {
                    await AuxAsync.DelayAsync(() => slot.SpawnDelay, () => _gamePauser.Paused == true, token);

                    if (token.IsCancellationRequested == true) break;

                    Vector3 spawnPosition = GetSpawnPosition();
                    Vector3 targetPosition = GetTargetPosition();
                    Vector3 movementDirection = (targetPosition - spawnPosition).normalized;
                    Quaternion spawnRotation = GetSpawnRotation();

                    WreckCache wreck = _wreckFactory.Create(slot.Type, spawnPosition, spawnRotation);
                    wreck.Transform.localScale = slot.Scale;
                    wreck.Transform.rotation = Quaternion.Euler(0f, 0f, AuxMath.DegreesPerRotation * AuxMath.RandomNormal);

                    MovementData data = new(slot.Speed, slot.Speed, 0f, spawnPosition, movementDirection, spawnRotation, null, 0f, 0f, null);
                    wreck.MovementSupplier.Supply(WreckMovement, data);

                    wreck.View.Escapable.SetEscapeDelay(EscapeDelay);
                    wreck.View.Escapable.Escaped += (s, e) => _wreckFactory.Release(slot.Type, wreck);
                    wreck.View.Destroyable.Destroyed += (s, e) => _wreckFactory.Release(slot.Type, wreck);

                    wreck.DamageDealer.Hit += (s, e) =>
                    {
                        e.Damageable.ApplyDamage(_config.GetCollisionDamage());
                        _audioPlayer.PlayOnceAsync(_config.CollisionAudio.Random, e.HitPosition, null, true).Forget();
                    };

                    WreckSpawned?.Invoke(this, new(wreck.View));
                }

                if (wave.WreckShower == true) ShowerEnded?.Invoke(this, EventArgs.Empty);
                else WaveEnded?.Invoke(this, EventArgs.Empty);
            }

            SpawnEnded?.Invoke(this, EventArgs.Empty);
            SpawnActive = false;
        }

        #endregion

        #region auxiliary methods

        private Vector3 GetSpawnPosition()
        {
            float xMin = _masterCameraHolder.ViewportLeftBound;
            float xMax = _masterCameraHolder.ViewportRightBound;
            float x = _config.GetSpawnWidth(xMin, xMax);
            float y = _masterCameraHolder.GetViewportUpperBoundWithOffset(SpawnPositionOffsetFactor);

            return new Vector3(x, y, 0f);
        }

        private Vector3 GetTargetPosition()
        {
            float xMin = _masterCameraHolder.ViewportLeftBound;
            float xMax = _masterCameraHolder.ViewportRightBound;
            float x = _config.GetSpawnWidth(xMin, xMax);
            float y = _masterCameraHolder.ViewportLowerBound;

            return new Vector3(x, y, 0f);
        }

        private Quaternion GetSpawnRotation() => Quaternion.Euler(0f, 0f, AuxMath.DegreesPerRotation * AuxMath.RandomNormal);

        #endregion
    }
}