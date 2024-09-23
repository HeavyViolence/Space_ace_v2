using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main;
using SpaceAce.Main.Factories.AmmoFactories;
using SpaceAce.Main.Factories.EnemyShipFactories;
using SpaceAce.Main.Factories.PlayerShipFactories;
using SpaceAce.UI;

using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Enemies
{
    public sealed class Enemy : IEntityViewProvider, IDisposable, IEquatable<Enemy>
    {
        private const float SpawnPositionOffsetFactor = 1.3f;

        public event EventHandler<DestroyedEventArgs> Defeated;

        private readonly EnemyConfig _config;
        private readonly EnemyShipFactory _enemyShipFactory;
        private readonly MasterCameraHolder _masterCameraHolder;
        private readonly GamePauser _gamePauser;
        private readonly Inventory _inventory = new();
        private readonly ShipCache _ship;
        private readonly CancellationTokenSource _shootingCancellation;

        public IEntityView View => _ship.View;
        public bool Alive { get; private set; } = true;

        public Enemy(EnemyConfig config,
                     AmmoFactory ammoFactory,
                     EnemyShipFactory enemyShipFactory,
                     MasterCameraHolder masterCameraHolder,
                     GamePauser gamePauser)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;

            if (ammoFactory is null) throw new ArgumentNullException();

            _masterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
            _enemyShipFactory = enemyShipFactory ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();

            IEnumerable<AmmoSet> assignedAmmo = ammoFactory.BatchCreateWithProbableQuality(config.GetAssignedAmmo());
            _inventory.TryAddItems(assignedAmmo, out _);

            float x = config.GetSpawnWidth(masterCameraHolder.ViewportLeftBound, masterCameraHolder.ViewportRightBound);
            float y = masterCameraHolder.ViewportUpperBound * SpawnPositionOffsetFactor;
            Vector3 spawnPosition = new(x, y, 0f);

            _ship = _enemyShipFactory.Create(config.ShipType, spawnPosition, Quaternion.Euler(0f, 0f, 180f));
            _ship.Shooting.BindInventoryAsync(_inventory).Forget();
            _ship.View.Destroyable.Destroyed += EnemyShipDestroyedEventHandler;

            _shootingCancellation = new();
            FireForeverAsync(_shootingCancellation.Token).Forget();
        }

        private async UniTask FireForeverAsync(CancellationToken token)
        {
            await UniTask.WaitUntil(() => _masterCameraHolder.InsideViewport(_ship.Transform.position) == true);

            float ammoSwitchTimer = 0f;
            float ammoSwitchDelay = _config.NextAmmoSwitchDelay;

            while (token.IsCancellationRequested == false)
            {
                await AuxAsync.DelayAsync(() => _config.NextShootingPause, () => _gamePauser.Paused == true, token);

                PausableCancellationTokenSource elapsed = new(_config.NextShootingDuration, () => _gamePauser.Paused == true);
                CancellationTokenSource elapsedOrCancelled = CancellationTokenSource.CreateLinkedTokenSource(elapsed.Token, token);

                DateTime t1 = DateTime.Now;

                await _ship.Shooting.FireAsync(this, elapsedOrCancelled.Token);
                await AuxAsync.DelayAsync(() => _config.NextShootingDuration, () => _gamePauser.Paused == true, elapsedOrCancelled.Token);

                DateTime t2 = DateTime.Now;
                ammoSwitchTimer += (float)(t2 - t1).TotalSeconds;

                if (ammoSwitchTimer > ammoSwitchDelay)
                {
                    ammoSwitchTimer = 0f;
                    ammoSwitchDelay = _config.NextAmmoSwitchDelay;

                    await _ship.Shooting.TrySwitchToNextAmmoAsync();
                }
            }
        }

        #region interfaces

        public void Dispose()
        {
            _ship.View.Destroyable.Destroyed -= EnemyShipDestroyedEventHandler;
            Defeated = null;

            if (Alive == true)
            {
                _enemyShipFactory.Release(_config.ShipType, _ship);

                _shootingCancellation?.Cancel();
                _shootingCancellation?.Dispose();
            }
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as Enemy);

        public bool Equals(Enemy other) => other is not null && View.ID == other.View.ID;

        public override int GetHashCode() => View.ID.GetHashCode();

        #endregion

        #region event handlers

        private void EnemyShipDestroyedEventHandler(object sender, DestroyedEventArgs e)
        {
            Alive = false;

            _enemyShipFactory.Release(_config.ShipType, _ship);

            _shootingCancellation?.Cancel();
            _shootingCancellation?.Dispose();

            Defeated?.Invoke(this, e);
        }

        #endregion
    }
}