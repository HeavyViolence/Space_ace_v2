using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main.Factories.AmmoFactories;
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

        private DateTime _startingItemsDispenseTime = DateTime.MaxValue.ToUniversalTime();

        public string SavedDataName => "Player starting items dispenser";

        public PlayerStartingItemsDispenser(Player player,
                                            AmmoFactory ammoFactory,
                                            ISavingSystem savedSystem,
                                            PlayerStartingItemsDispenserConfig config)
        {
            _player = player ?? throw new ArgumentNullException();
            _ammoFactory = ammoFactory ?? throw new ArgumentNullException();
            _savingSystem = savedSystem ?? throw new ArgumentNullException();

            if (config == null) throw new ArgumentNullException();
            _config = config;
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);
            DispenseStartingItemsAsync().Forget();
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
                _startingItemsDispenseTime = DateTime.MaxValue.ToUniversalTime();
            }
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && other.SavedDataName == SavedDataName;

        public override int GetHashCode() => SavedDataName.GetHashCode();

        #endregion

        private async UniTaskVoid DispenseStartingItemsAsync()
        {
            if (_startingItemsDispenseTime < DateTime.UtcNow) return;

            await UniTask.NextFrame();

            _player.Wallet.AddCredits(_config.StartingCredits);

            List<AmmoSet> startingAmmo = new();

            foreach (var ammoRequest in _config.GetStartingAmmoRequests())
            {
                AmmoSet ammo = _ammoFactory.Create(ammoRequest);
                startingAmmo.Add(ammo);
            }

            _player.Inventory.TryAddItems(startingAmmo, out _);

            _startingItemsDispenseTime = DateTime.UtcNow;
            SavingRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}