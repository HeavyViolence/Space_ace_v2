using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Main;
using SpaceAce.Main.Factories.MeteorFactories;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Meteors
{
    public sealed class MeteorSpawner : IInitializable, IDisposable
    {
        private const float SpawnPositionOffsetFactor = 1.3f;
        private const float EscapeDelay = 3f;

        public event EventHandler SpawnStarted, SpawnEnded;
        public event EventHandler WaveStarted, WaveEnded;
        public event EventHandler MeteorShowerStarted, MeteorShowerEnded;
        public event EventHandler<MeteorSpawnedEventArgs> MeteorSpawned;
        public event EventHandler MeteorEscaped, MeteorDestroyed;

        private readonly MeteorSpawnerConfig _config;
        private readonly MeteorFactory _meteorFactory;
        private readonly GameStateLoader _gameStateLoader;
        private readonly LevelCompleter _levelCompleter;
        private readonly GamePauser _gamePauser;
        private readonly MasterCameraHolder _masterCameraHolder;

        private CancellationTokenSource _spawnCancellation;

        public bool SpawnActive { get; private set; } = false;

        public MeteorSpawner(MeteorSpawnerConfig config,
                             MeteorFactory meteorFactory,
                             GameStateLoader gameStateLoader,
                             LevelCompleter levelCompleter,
                             GamePauser gamePauser,
                             MasterCameraHolder masterCameraHolder)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;

            _meteorFactory = meteorFactory ?? throw new ArgumentNullException();
            _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            _levelCompleter = levelCompleter ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _masterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
        }

        #region interfaces

        public void Initialize()
        {
            _gameStateLoader.LevelLoaded += async (s, e) =>
            {
                _spawnCancellation = new();
                await LevelLoadedEventHandlerAsync(e, _spawnCancellation.Token);
            };

            _levelCompleter.LevelConcluded += LevelConcludedEventHandler;
        }

        public void Dispose()
        {
            _gameStateLoader.LevelLoaded -= async (s, e) =>
            {
                _spawnCancellation = new();
                await LevelLoadedEventHandlerAsync(e, _spawnCancellation.Token);
            };

            _levelCompleter.LevelConcluded -= LevelConcludedEventHandler;
        }

        #endregion

        #region event handlers

        private async UniTask LevelLoadedEventHandlerAsync(LevelLoadedEventArgs e, CancellationToken token)
        {
            await SpawnAsync(e.LevelIndex, token);
        }

        private void LevelConcludedEventHandler(object sender, LevelEndedEventArgs e)
        {
            _spawnCancellation.Cancel();
            _spawnCancellation.Dispose();
            _spawnCancellation = null;
        }

        private async UniTask SpawnAsync(int levelIndex, CancellationToken token)
        {
            SpawnStarted?.Invoke(this, EventArgs.Empty);
            SpawnActive = true;

            while (token.IsCancellationRequested == false)
            {
                MeteorWave wave = _config.NextWave(levelIndex);

                if (wave.MeteorShower == true) MeteorShowerStarted?.Invoke(this, EventArgs.Empty);
                else WaveStarted?.Invoke(this, EventArgs.Empty);

                foreach (MeteorWaveSlot slot in wave)
                {
                    await WaitForDelayAsync(slot.SpawnDelay, token);

                    Vector3 spawnPosition = GetSpawnPosition();
                    Vector3 targetPosition = GetTargetPosition();
                    Vector3 movementDirection = (targetPosition - spawnPosition).normalized;

                    CachedMeteor meteor = _meteorFactory.Create(slot.Type, spawnPosition);
                    meteor.Transform.localScale = slot.Scale;
                    meteor.Transform.rotation = Quaternion.Euler(0f, 0f, AuxMath.DegreesPerRotation * AuxMath.RandomNormal);

                    MovementData data = new(slot.Speed, slot.Speed, 0f, spawnPosition, movementDirection, meteor.Transform.rotation, null, 0f, 0f, null);
                    meteor.MovementSupplier.Supply(MeteorMovement, data);

                    meteor.View.Escapable.SetEscapeDelay(EscapeDelay);

                    meteor.View.Escapable.Escaped += (s, e) =>
                    {
                        _meteorFactory.Release(slot.Type, meteor);
                        MeteorEscaped?.Invoke(this, EventArgs.Empty);
                    };

                    meteor.View.Destroyable.Destroyed += (s, e) =>
                    {
                        _meteorFactory.Release(slot.Type, meteor);
                        MeteorDestroyed?.Invoke(this, EventArgs.Empty);
                    };

                    MeteorSpawned?.Invoke(this, new(meteor.View));
                }

                if (wave.MeteorShower == true) MeteorShowerEnded?.Invoke(this, EventArgs.Empty);
                else WaveEnded?.Invoke(this, EventArgs.Empty);
            }

            SpawnEnded?.Invoke(this, EventArgs.Empty);
            SpawnActive = false;
        }

        #endregion

        #region auxiliary methods

        private async UniTask WaitForDelayAsync(float delay, CancellationToken token)
        {
            await UniTask.WaitUntil(() => _gamePauser.Paused == false);

            float timer = 0f;

            while (timer < delay)
            {
                if (token.IsCancellationRequested == true) return;

                timer += Time.deltaTime;

                await UniTask.Yield();
            }
        }

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

        private MovementBehaviour MeteorMovement => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        #endregion
    }
}