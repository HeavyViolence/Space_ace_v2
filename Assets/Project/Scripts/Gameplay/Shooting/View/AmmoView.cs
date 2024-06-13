using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main.Localization;

using System;

namespace SpaceAce.Gameplay.Shooting
{
    public sealed record AmmoView
    {
        public event EventHandler<IntValueChangedEventArgs> AmountChanged;
        public event EventHandler AmmoSwitched, OutOfAmmo;

        private readonly Localizer _localizedizer;
        private AmmoSet _ammo = null;

        public int Amount => _ammo is null ? 0 : _ammo.Amount;
        public float Damage => _ammo is null ? 0f : _ammo.Damage;

        public AmmoView(Localizer localizer, AmmoSet ammo)
        {
            _localizedizer = localizer ?? throw new ArgumentNullException();

            if (ammo is not null)
            {
                _ammo = ammo;
                _ammo.AmountChanged += (s, e) => AmountChanged(s, e);
                _ammo.Depleted += (_, _) => Reset();
            }
        }

        public async UniTask<string> GetSizeCodeAsync() =>
            _ammo is null ? await _localizedizer.GetLocalizedStringAsync("Ammo", "None")
                          : await _ammo.GetSizeCodeAsync();

        public async UniTask<string> GetTypeCodeAsync() =>
            _ammo is null ? await _localizedizer.GetLocalizedStringAsync("Ammo", "None")
                          : await _ammo.GetTypeCodeAsync();

        public void Rebind(AmmoSet ammo)
        {
            if (ammo is null) throw new ArgumentNullException();

            if (_ammo is not null)
            {
                _ammo.AmountChanged -= (s, e) => AmountChanged(s, e);
                _ammo.Depleted -= (_, _) => Reset();
            }

            _ammo = ammo;
            _ammo.AmountChanged += (s, e) => AmountChanged(s, e);
            _ammo.Depleted += (_, _) => Reset();

            AmmoSwitched?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            if (_ammo is null) return;

            _ammo.AmountChanged -= (s, e) => AmountChanged(s, e);
            _ammo.Depleted -= (_, _) => Reset();
            _ammo = null;

            OutOfAmmo?.Invoke(this, EventArgs.Empty);
        }
    }
}