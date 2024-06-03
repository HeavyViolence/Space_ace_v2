using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Experience;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Localization;

using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Shooting
{
    public sealed class Shooting : MonoBehaviour, IShooterView, IExperienceSource
    {
        public event EventHandler ShootingStarted, ShootingStopped;
        public event EventHandler Overheated, CooledDown;

        public event EventHandler<FloatValueChangedEventArgs> HeatValueChanged, HeatCapacityChanged;

        public event EventHandler<WeaponChangedEventArgs> WeaponChanged;
        public event EventHandler<AmmoChangedEventArgs> AmmoChanged;
        public event EventHandler<OutOfAmmoEventArgs> OutOfAmmo;

        [SerializeField]
        private ShootingConfig _config;

        private readonly List<Gun> _availableGuns = new();

        private List<AmmoSet> _availableAmmo;
        private List<Gun> _activeGuns;
        private List<AmmoSet> _ammoForActiveWeapons;

        private AnimationCurve HeatLossFactorCurve => _config.HeatLossFactorCurve;

        private float _baseHeatLossRate;

        private GamePauser _gamePauser;
        private AudioPlayer _audioPlayer;
        private Localizer _localizer;
        private Inventory _userInventory;
        private Transform _transform;

        private bool _weaponsSwitchEnabled = true;

        public float MinInitialHeatCapacity => _config.MinInitialHeatCapacity;
        public float MaxInitialHeatCapacity => _config.MaxInitialHeatCapacity;
        public float RandomInitialHeatCapacity => _config.RandomInitialHeatCapacity;

        public float MinInitialBaseHeatLossRate => _config.MinInitialBaseHeatLossRate;
        public float MaxInitialBaseHeatLossRate => _config.MaxInitialBaseHeatLossRate;
        public float RandomInitialBaseHeatLossRate => _config.RandomInitialBaseHeatLossRate;

        public float MinInitialOverheatDuration => _config.MinInitialOverheatDuration;
        public float MaxInitialOverheatDuration => _config.MaxInitialOverheatDuration;
        public float RandomInitialOverheatDuration => _config.RandomInitialOverheatDuration;

        public float MinSmallWeaponsSwitchDuration => _config.MinSmallWeaponsSwitchDuration;
        public float MaxSmallweaponsSwitchDuration => _config.MaxSmallWeaponsSwitchDuration;
        public float RandomSmallWeaponsSwitchDuration => _config.RandomSmallWeaponsSwitchDuration;

        public float MinMediumWeaponsSwitchDuration => _config.MinMediumWeaponsSwitchDuration;
        public float MaxMediumWeaponsSwitchDuration => _config.MaxMediumWeaponsSwitchDuration;
        public float RandomMediumWeaponsSwitchDuration => _config.RandomMediumWeaponsSwitchDuration;

        public float MinLargeWeaponsSwitchDuration => _config.MinLargeWeaponsSwitchDuration;
        public float MaxLargeWeaponsSwitchDuration => _config.MaxLargeWeaponsSwitchDuration;
        public float RandomLargeWeaponsSwitchDuration => _config.RandomLargeWeaponsSwitchDuration;

        public float MinAmmoSwitchDuration => _config.MinInitialAmmoSwitchDuration;
        public float MaxAmmoSwitchDuration => _config.MaxInitialAmmoSwitchDuration;
        public float RandomAmmoSwitchDuration => _config.RandomInitialAmmoSwitchDuration;

        public AmmoSet ActiveAmmo { get; private set; }

        private CancellationTokenSource _overheatCancellation;

        private float _heat;
        public float Heat
        {
            get => _heat;

            private set
            {
                HeatValueChanged?.Invoke(this, new(_heat, value));
                _heat = Mathf.Clamp(value, 0f, _heatCapacity);

                if (HeatNormalized == 1f) PerformOverheatAsync().Forget();
            }
        }

        private float _heatCapacity;
        public float HeatCapacity
        {
            get => _heatCapacity;

            private set
            {
                HeatCapacityChanged?.Invoke(this, new(_heatCapacity, value));
                _heatCapacity = value;
            }
        }

        public float HeatNormalized => Heat / HeatCapacity;
        public float HeatPercentage => HeatNormalized * 100f;
        public float OverheatDuration { get; private set; }

        public bool Overheat { get; private set; }
        public bool Firing => FirstActiveGunView is null ? false : FirstActiveGunView.Firing;

        public Size ActiveWeaponsSize { get; private set; } = Size.None;

        public int ActiveAmmoIndex { get; private set; }
        public int AmmoCountForActiveWeapons => _ammoForActiveWeapons.Count;

        public IGunView FirstActiveGunView => _activeGuns?[0];

        [Inject]
        private void Construct(GamePauser gamePauser,
                               AudioPlayer audioPlayer,
                               Localizer localizer)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException();
            _localizer = localizer ?? throw new ArgumentNullException();
        }

        private void Awake()
        {
            _transform = transform;
            _availableGuns.AddRange(gameObject.GetComponentsInChildren<Gun>());
        }

        private void OnEnable()
        {
            HeatCapacity = RandomInitialHeatCapacity;
            Heat = 0f;
            _baseHeatLossRate = RandomInitialBaseHeatLossRate;
            OverheatDuration = RandomInitialOverheatDuration;
            Overheat = false;
        }

        private void OnDisable()
        {
            _userInventory.ContentChanged -= async (_, _) => await UpdateAvailableAmmoAsync();
        }

        private void Update()
        {
            if (_gamePauser.Paused == true || Heat == 0f || Overheat == true) return;

            Heat = Mathf.Clamp(Heat - GetHeatLossThisFrame(), 0f, HeatCapacity);
        }

        private float GetHeatLossThisFrame()
        {
            float factor = HeatLossFactorCurve.Evaluate(HeatNormalized);
            return _baseHeatLossRate * factor * Time.deltaTime;
        }

        public float GetDamagePerSecond()
        {
            float damagePerSecond = 0f;

            foreach (Gun gun in _activeGuns)
                damagePerSecond += gun.FireRate * ActiveAmmo.Damage;

            return damagePerSecond;
        }

        public void Fire(object shooter, CancellationToken token = default)
        {
            if (shooter is null) throw new ArgumentNullException();

            if (Firing == true || Overheat == true) return;

            if (ActiveAmmo is null)
            {
                _audioPlayer.PlayOnceAsync(_config.EmptyAmmoAudio.Random, _transform.position, _transform, true).Forget();
                return;
            }

            ShootingStarted?.Invoke(this, EventArgs.Empty);
            _overheatCancellation = new();

            foreach (Gun gun in _activeGuns)
                gun.FireAsync(shooter, ActiveAmmo, token, _overheatCancellation.Token).Forget();
        }

        private async UniTask PerformOverheatAsync()
        {
            if (Overheat == true) return;

            ShootingStopped?.Invoke(this, EventArgs.Empty);

            Overheat = true;
            Overheated?.Invoke(this, EventArgs.Empty);

            _overheatCancellation?.Cancel();
            _overheatCancellation?.Dispose();
            _overheatCancellation = null;

            _audioPlayer.PlayOnceAsync(_config.OverheatAudio.Random, _transform.position, _transform, true).Forget();

            float timer = 0f;

            while (timer < OverheatDuration)
            {
                timer += Time.deltaTime;

                Heat = Mathf.Lerp(HeatCapacity, 0f, timer / OverheatDuration);

                await UniTask.WaitUntil(() => _gamePauser.Paused == false);
                await UniTask.Yield();
            }

            Heat = 0f;
            Overheat = false;
            CooledDown?.Invoke(this, EventArgs.Empty);
        }

        public void BindInventory(Inventory inventory)
        {
            _userInventory = inventory ?? throw new ArgumentNullException();
            _availableAmmo = GetAvailableAmmo();

            TrySwitchToWorkingWeapons().Forget();

            _userInventory.ContentChanged += async (_, _) => await UpdateAvailableAmmoAsync();
        }

        private async UniTask<bool> TrySwitchToWorkingWeapons()
        {
            if (await TrySwitchWeaponsAsync(Size.Small) == true) return true;
            if (await TrySwitchWeaponsAsync(Size.Medium) == true) return true;
            if (await TrySwitchWeaponsAsync(Size.Large) == true) return true;

            return false;
        }

        public async UniTask<bool> TrySwitchWeaponsAsync(Size size)
        {
            if (Firing == true || _weaponsSwitchEnabled == false || ActiveWeaponsSize == size) return false;

            if (_activeGuns is not null)
                foreach (Gun gun in _activeGuns)
                    gun.ShotFired -= ShotFiredEventHandler;

            _weaponsSwitchEnabled = false;

            List<Gun> activeGuns = new();

            foreach (Gun gun in _availableGuns)
                if (gun.AmmoSize == size)
                    activeGuns.Add(gun);

            List<AmmoSet> ammoSetsForActiveGuns = GetAmmo(size);

            if (activeGuns.Count > 0 && ammoSetsForActiveGuns.Count > 0)
            {
                AmmoSet previousAmmo = ActiveAmmo;

                _activeGuns = activeGuns;
                WeaponChanged?.Invoke(this, new(FirstActiveGunView));

                foreach (Gun gun in _activeGuns)
                    gun.ShotFired += ShotFiredEventHandler;

                _ammoForActiveWeapons = ammoSetsForActiveGuns;
                ActiveAmmoIndex = 0;
                ActiveAmmo = _ammoForActiveWeapons[0];
                ActiveWeaponsSize = size;

                if (AmmoCountForActiveWeapons == 0)
                {
                    string outOfAmmoWarning = await _localizer.GetLocalizedStringAsync("Ammo", "Out of ammo warning", this);
                    OutOfAmmo?.Invoke(this, new(outOfAmmoWarning));
                }
                else
                {
                    AmmoChanged?.Invoke(this, new(ActiveAmmo, previousAmmo));
                }

                switch (size)
                {
                    case Size.Small:
                        {
                            if (_config.SmallWeaponsSwitchAudio == null)
                                await UniTask.WaitForSeconds(RandomSmallWeaponsSwitchDuration);
                            else
                                await _audioPlayer.PlayOnceAsync(_config.SmallWeaponsSwitchAudio.Random, _transform.position, _transform, true);

                            break;
                        }
                    case Size.Medium:
                        {
                            if (_config.MediumWeaponsSwitchAudio == null)
                                await UniTask.WaitForSeconds(RandomMediumWeaponsSwitchDuration);
                            else
                                await _audioPlayer.PlayOnceAsync(_config.MediumWeaponsSwitchAudio.Random, _transform.position, _transform, true);

                            break;
                        }
                    case Size.Large:
                        {
                            if (_config.LargeWeaponsSwitchAudio == null)
                                await UniTask.WaitForSeconds(RandomLargeWeaponsSwitchDuration);
                            else
                                await _audioPlayer.PlayOnceAsync(_config.LargeWeaponsSwitchAudio.Random, _transform.position, _transform, true);

                            break;
                        }
                }

                _weaponsSwitchEnabled = true;
                return true;
            }

            _weaponsSwitchEnabled = true;
            return false;
        }

        private List<AmmoSet> GetAvailableAmmo()
        {
            List<AmmoSet> availableAmmo = new();

            foreach (IItem item in _userInventory.GetItems())
                if (item is AmmoSet ammo)
                    availableAmmo.Add(ammo);

            return availableAmmo;
        }

        private List<AmmoSet> GetAmmo(Size size)
        {
            List<AmmoSet> activeAmmo = new();

            foreach (AmmoSet ammo in _availableAmmo)
                if (ammo.Size == size)
                    activeAmmo.Add(ammo);

            return activeAmmo;
        }

        public async UniTask<bool> TrySwitchToNextAmmoAsync()
        {
            if (Firing == true) return false;
            if (AmmoCountForActiveWeapons <= 1) return false;

            if (_config.AmmoSwitchAudio == null) await UniTask.WaitForSeconds(RandomAmmoSwitchDuration);
            else await _audioPlayer.PlayOnceAsync(_config.AmmoSwitchAudio.Random, _transform.position, _transform, true);

            AmmoSet previousAmmo = ActiveAmmo;

            ActiveAmmoIndex = ActiveAmmoIndex++ % AmmoCountForActiveWeapons;
            ActiveAmmo = _ammoForActiveWeapons[ActiveAmmoIndex];

            if (AmmoCountForActiveWeapons == 0)
            {
                string outOfAmmoWarning = await _localizer.GetLocalizedStringAsync("Ammo", "Out of ammo warning", this);
                OutOfAmmo?.Invoke(this, new(outOfAmmoWarning));
            }
            else
            {
                AmmoChanged?.Invoke(this, new(ActiveAmmo, previousAmmo));
            }

            return true;
        }

        public async UniTask<bool> TrySwitchToPreviousAmmoAsync()
        {
            if (Firing == true) return false;
            if (AmmoCountForActiveWeapons <= 1) return false;

            if (_config.AmmoSwitchAudio == null) await UniTask.WaitForSeconds(RandomAmmoSwitchDuration);
            else await _audioPlayer.PlayOnceAsync(_config.AmmoSwitchAudio.Random, _transform.position, _transform, true);

            if (ActiveAmmoIndex == 0) ActiveAmmoIndex = AmmoCountForActiveWeapons - 1;
            else ActiveAmmoIndex--;

            AmmoSet previousAmmo = ActiveAmmo;
            ActiveAmmo = _ammoForActiveWeapons[ActiveAmmoIndex];

            if (AmmoCountForActiveWeapons == 0)
            {
                string outOfAmmoWarning = await _localizer.GetLocalizedStringAsync("Ammo", "Out of ammo warning", this);
                OutOfAmmo?.Invoke(this, new(outOfAmmoWarning));
            }
            else
            {
                AmmoChanged?.Invoke(this, new(ActiveAmmo, previousAmmo));
            }

            return true;
        }

        #region event handlers

        private async UniTask UpdateAvailableAmmoAsync()
        {
            _availableAmmo = GetAvailableAmmo();
            _ammoForActiveWeapons = GetAmmo(ActiveWeaponsSize);

            if (Firing == false && ActiveAmmo.Amount == 0)
            {
                bool switchedFromDepletedAmmo = await TrySwitchToNextAmmoAsync();
                if (switchedFromDepletedAmmo == false) await TrySwitchToWorkingWeapons();
            }
        }

        private void ShotFiredEventHandler(object sender, ShotFiredEventArgs e)
        {
            Heat += e.ShotResult.Heat;
        }

        #endregion

        public float GetExperience() => GetDamagePerSecond();
    }
}