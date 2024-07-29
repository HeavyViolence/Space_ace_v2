using Cysharp.Threading.Tasks;

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

using UnityEngine;

namespace SpaceAce.Gameplay.Enemies
{
    public sealed class Enemy : IEntityViewProvider, IDisposable, IEquatable<Enemy>
    {
        private const float SpawnPositionOffsetFactor = 1.3f;

        public event EventHandler<DestroyedEventArgs> Defeated;

        private readonly EnemyConfig _config;
        private readonly EnemyShipFactory _enemyShipFactory;
        private readonly Inventory _inventory = new();
        private readonly ShipCache _ship;

        public IEntityView View => _ship.View;
        public bool Alive { get; private set; } = true;

        public Enemy(EnemyConfig config,
                     AmmoFactory ammoFactory,
                     EnemyShipFactory enemyShipFactory,
                     MasterCameraHolder masterCameraHolder)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;

            if (ammoFactory is null) throw new ArgumentNullException();
            if (masterCameraHolder is null) throw new ArgumentNullException();

            _enemyShipFactory = enemyShipFactory ?? throw new ArgumentNullException();

            IEnumerable<AmmoSet> assignedAmmo = ammoFactory.BatchCreateWithProbableQuality(config.GetAssignedAmmo());
            _inventory.TryAddItems(assignedAmmo, out _);

            float x = config.GetSpawnWidth(masterCameraHolder.ViewportLeftBound, masterCameraHolder.ViewportRightBound);
            //float y = masterCameraHolder.ViewportUpperBound * SpawnPositionOffsetFactor;
            float y = 0f;
            Vector3 spawnPosition = new(x, y, 0f);

            _ship = _enemyShipFactory.Create(config.ShipType, spawnPosition, Quaternion.Euler(0f, 0f, 180f));
            _ship.Shooting.BindInventoryAsync(_inventory).Forget();
            _ship.View.Destroyable.Destroyed += EnemyShipDestroyedEventHandler;
        }

        #region interfaces

        public void Dispose()
        {
            _ship.View.Destroyable.Destroyed -= EnemyShipDestroyedEventHandler;
            Defeated = null;

            if (Alive == true) _enemyShipFactory.Release(_config.ShipType, _ship);
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as Enemy);

        public bool Equals(Enemy other) => other is not null && View.ID == other.View.ID;

        public override int GetHashCode() => View.ID.GetHashCode();

        #endregion

        #region event handlers

        private void EnemyShipDestroyedEventHandler(object sender, DestroyedEventArgs e)
        {
            Defeated?.Invoke(this, e);
            Alive = false;
            _enemyShipFactory.Release(_config.ShipType, _ship);
        }

        #endregion
    }
}