using Newtonsoft.Json;

using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main.Factories;
using SpaceAce.Main.Saving;

using System;
using System.Collections.Generic;

using Zenject;

namespace SpaceAce.Gameplay.Players
{
    public sealed class PlayerStartingItemsDispenser : IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly Player _player;
        private readonly AmmoFactory _ammoFactory;
        private readonly ISavingSystem _savingSystem;
        private readonly PlayerStartingItemsDispenserConfig _config;

        private DateTime _startingItemsDispenseTime = DateTime.MaxValue;

        public string ID => "Player starting items dispenser";

        public PlayerStartingItemsDispenser(Player player,
                                            AmmoFactory ammoFactory,
                                            ISavingSystem savedSystem,
                                            PlayerStartingItemsDispenserConfig config)
        {
            _player = player ??
                throw new ArgumentNullException(nameof(player),
                $"Attempted to pass an empty {typeof(Player)}!");

            _ammoFactory = ammoFactory ??
                throw new ArgumentNullException(nameof(ammoFactory),
                $"Attempted to pass an empty {typeof(AmmoFactory)}!");

            _savingSystem = savedSystem ??
                throw new ArgumentNullException(nameof(savedSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");

            if (config == null)
                throw new ArgumentNullException(nameof(config),
                    $"Attempted to pass an empty {typeof(PlayerStartingItemsDispenserConfig)}!");

            _config = config;
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);

            if (_startingItemsDispenseTime < DateTime.Now) return;

            AddCredits();
            AddAmmo();

            _startingItemsDispenseTime = DateTime.Now;
            SavingRequested?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);
        }

        public string GetState() => JsonConvert.SerializeObject(_startingItemsDispenseTime);

        public void SetState(string state)
        {
            try
            {
                _startingItemsDispenseTime = JsonConvert.DeserializeObject<DateTime>(state);
            }
            catch (Exception)
            {
                _startingItemsDispenseTime = DateTime.MaxValue;
            }
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.ID == ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion

        public void AddCredits()
        {
            _player.Wallet.AddCredits(_config.StartingCredits);
        }

        public void AddAmmo()
        {
            List<AmmoStack> startingAmmo = new();

            foreach (var ammoRequest in _config.GetStartingAmmoRequests())
            {
                AmmoStack ammo = _ammoFactory.Create(ammoRequest);
                startingAmmo.Add(ammo);
            }

            _player.Inventory.Add(startingAmmo);
        }
    }
}