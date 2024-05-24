using SpaceAce.Gameplay.Bombs;
using SpaceAce.Main.Factories.BombFactories;
using SpaceAce.Gameplay.Levels;
using SpaceAce.Main;
using SpaceAce.Gameplay.Movement;

using System;
using System.Threading;

using UnityEngine;

using Cysharp.Threading.Tasks;

using Zenject;

public sealed class BombSpawner : IInitializable, IDisposable
{
    private const float SpawnPositionOffsetFactor = 1.3f;
    private const float EscapeDelay = 3f;

    public event EventHandler SpawnStarted, SpawnEnded;
    public event EventHandler WaveStarted, WaveEnded;
    public event EventHandler<BombSpawnedEventArgs> BombSpawned;

    private readonly BombSpawnerConfig _config;
    private readonly BombFactory _bombFactory;
    private readonly GameStateLoader _gameStateLoader;
    private readonly LevelCompleter _levelCompleter;
    private readonly GamePauser _gamePauser;
    private readonly MasterCameraHolder _masterCameraHolder;

    private CancellationTokenSource _spawnCancellation;

    public bool SpawnActive { get; private set; } = false;

    public BombSpawner(BombSpawnerConfig config,
                       BombFactory bombFactory,
                       GameStateLoader gameStateLoader,
                       LevelCompleter levelCompleter,
                       GamePauser gamePauser,
                       MasterCameraHolder masterCameraHolder)
    {
        if (config == null) throw new ArgumentNullException();
        _config = config;

        _bombFactory = bombFactory ?? throw new ArgumentNullException();
        _gameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
        _levelCompleter = levelCompleter ?? throw new ArgumentNullException();
        _gamePauser = gamePauser ?? throw new ArgumentNullException();
        _masterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
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
            BombWave wave = _config.NextWave(levelIndex);
            WaveStarted?.Invoke(this, EventArgs.Empty);

            foreach (BombWaveSlot slot in wave)
            {
                await WaitForDelayAsync(slot.SpawnDelay, token);

                Vector3 spawnPosition = GetSpawnPosition();
                Vector3 targetPosition = GetTargetPosition(spawnPosition.x);
                Vector3 movementDirection = (targetPosition - spawnPosition).normalized;

                BombCache bomb = _bombFactory.Create(slot.Size, spawnPosition, Quaternion.identity);

                MovementData data = new(slot.Speed, slot.Speed, 0f, spawnPosition, movementDirection, Quaternion.identity, null, 0f, 0f, null);
                bomb.MovementSupplier.Supply(MeteorMovement, data);

                bomb.View.Escapable.SetEscapeDelay(EscapeDelay);
                bomb.View.Escapable.Escaped += (s, e) => _bombFactory.Release(slot.Size, bomb);
                bomb.View.Destroyable.Destroyed += (s, e) => _bombFactory.Release(slot.Size, bomb);

                BombSpawned?.Invoke(this, new(bomb.View));

                await UniTask.WaitUntil(() => _gamePauser.Paused == false, PlayerLoopTiming.Update, token);

                if (token.IsCancellationRequested == true) break;
            }

            WaveEnded?.Invoke(this, EventArgs.Empty);
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

    private Vector3 GetTargetPosition(float spawnWidth)
    {
        float y = _masterCameraHolder.ViewportLowerBound;
        return new Vector3(spawnWidth, y, 0f);
    }

    private MovementBehaviour MeteorMovement => delegate (Rigidbody2D body, ref MovementData data)
    {
        body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
    };

    #endregion
}
