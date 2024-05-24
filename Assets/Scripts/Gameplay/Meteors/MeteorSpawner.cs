using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
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
        public event EventHandler ShowerStarted, ShowerEnded;
        public event EventHandler<MeteorSpawnedEventArgs> MeteorSpawned;

        private readonly MeteorSpawnerConfig _config;
        private readonly MeteorFactory _meteorFactory;
        private readonly GameStateLoader _gameStateLoader;
        private readonly LevelCompleter _levelCompleter;
        private readonly GamePauser _gamePauser;
        private readonly MasterCameraHolder _masterCameraHolder;
        private readonly AudioPlayer _audioPlayer;

        private CancellationTokenSource _spawnCancellation;

        public bool SpawnActive { get; private set; } = false;

        public MeteorSpawner(MeteorSpawnerConfig config,
                             MeteorFactory meteorFactory,
                             GameStateLoader gameStateLoader,
                             LevelCompleter levelCompleter,
                             GamePauser gamePauser,
                             MasterCameraHolder masterCameraHolder,
                             AudioPlayer audioPlayer)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;

            _meteorFactory = meteorFactory ?? throw new ArgumentNullException();
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

        private async UniTask SpawnAsync(int levelIndex, CancellationToken token)
        {
            SpawnStarted?.Invoke(this, EventArgs.Empty);
            SpawnActive = true;

            while (token.IsCancellationRequested == false)
            {
                MeteorWave wave = _config.NextWave(levelIndex);

                if (wave.MeteorShower == true) ShowerStarted?.Invoke(this, EventArgs.Empty);
                else WaveStarted?.Invoke(this, EventArgs.Empty);

                foreach (MeteorWaveSlot slot in wave)
                {
                    await WaitForDelayAsync(slot.SpawnDelay, token);

                    Vector3 spawnPosition = GetSpawnPosition();
                    Vector3 targetPosition = GetTargetPosition();
                    Vector3 movementDirection = (targetPosition - spawnPosition).normalized;
                    Quaternion spawnRotation = GetSpawnRotation();

                    MeteorCache meteor = _meteorFactory.Create(slot.Type, spawnPosition, spawnRotation);
                    meteor.Transform.localScale = slot.Scale;

                    MovementData data = new(slot.Speed, slot.Speed, 0f, spawnPosition, movementDirection, spawnRotation, null, 0f, 0f, null);
                    meteor.MovementSupplier.Supply(MeteorMovement, data);

                    meteor.View.Escapable.SetEscapeDelay(EscapeDelay);
                    meteor.View.Escapable.Escaped += (s, e) => _meteorFactory.Release(slot.Type, meteor);
                    meteor.View.Destroyable.Destroyed += (s, e) => _meteorFactory.Release(slot.Type, meteor);

                    meteor.DamageDealer.Hit += (s, e) =>
                    {
                        e.DamageReceiver.ApplyDamage(_config.GetCollisionDamage());
                        _audioPlayer.PlayOnceAsync(_config.CollisionAudio.Random, e.HitPosition, null, true).Forget();
                    };

                    MeteorSpawned?.Invoke(this, new(meteor.View));

                    await UniTask.WaitUntil(() => _gamePauser.Paused == false, PlayerLoopTiming.Update, token);

                    if (token.IsCancellationRequested == true) break;
                }

                if (wave.MeteorShower == true) ShowerEnded?.Invoke(this, EventArgs.Empty);
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

        private Quaternion GetSpawnRotation() => Quaternion.Euler(0f, 0f, AuxMath.DegreesPerRotation * AuxMath.RandomNormal);

        private MovementBehaviour MeteorMovement => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        #endregion
    }
}