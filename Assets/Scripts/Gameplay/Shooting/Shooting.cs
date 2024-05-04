using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
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
    public sealed class Shooting : MonoBehaviour, IShooterView, IEMPTarget
    {
        public event EventHandler ShootingStarted, ShootingStopped;
        public event EventHandler Overheated, CooledDown;

        public event EventHandler<WeaponChangedEventArgs> WeaponChanged;
        public event EventHandler<AmmoChangedEventArgs> AmmoChanged;
        public event EventHandler<OutOfAmmoEventArgs> OutOfAmmo;

        private readonly List<IGun> _availableGuns = new();
        private List<AmmoSet> _availableAmmo;

        private List<IGun> _activeGuns;

        private List<AmmoSet> _ammoForActiveWeapons;

        [SerializeField]
        private ShootingConfig _config;

        private AnimationCurve HeatLossFactorCurve => _config.HeatLossFactorCurve;

        private float _baseHeatLossRate;

        private GamePauser _gamePauser;
        private AudioPlayer _audioPlayer;
        private MasterCameraShaker _masterCameraShaker;
        private Localizer _localizer;
        private Inventory _userInventory;

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

        public float Heat { get; private set; }
        public float HeatCapacity { get; private set; }
        public float HeatNormalized => Heat / HeatCapacity;
        public float HeatPercentage => HeatNormalized * 100f;
        public float OverheatDuration { get; private set; }

        public bool Overheat { get; private set; }
        public bool Firing { get; private set; }
        public bool FirstShotInLine { get; private set; }

        public Size ActiveWeaponsSize { get; private set; } = Size.None;

        public int ActiveAmmoIndex { get; private set; }
        public int AmmoCountForActiveWeapons => _ammoForActiveWeapons.Count;

        public IGun FirstActiveGun => _activeGuns[0];

        [Inject]
        private void Construct(GamePauser gamePauser,
                               AudioPlayer audioPlayer,
                               MasterCameraShaker masterCameraShaker,
                               Localizer localizer)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException();
            _masterCameraShaker = masterCameraShaker ?? throw new ArgumentNullException();
            _localizer = localizer ?? throw new ArgumentNullException();
        }

        private void Awake()
        {
            _availableGuns.AddRange(gameObject.GetComponentsInChildren<IGun>());
        }

        private void OnEnable()
        {
            Heat = 0f;
            HeatCapacity = RandomInitialHeatCapacity;
            _baseHeatLossRate = RandomInitialBaseHeatLossRate;
            OverheatDuration = RandomInitialOverheatDuration;
            Overheat = false;
            Firing = false;
        }

        private void OnDisable()
        {
            _userInventory.ContentChanged -= async (sender, args) => await InventoryContentChangedEventHandlerAsync(sender, args);
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

            foreach (IGun gun in _activeGuns) damagePerSecond += gun.FireRate / _activeGuns.Count * ActiveAmmo.Damage;

            return damagePerSecond;
        }

        public async UniTask FireAsync(object user, CancellationToken token)
        {
            if (Overheat == true) return;
            if (Firing == true) return;

            if (AmmoCountForActiveWeapons == 0)
            {
                if (_config.EmptyAmmoAudio != null)
                {
                    await _audioPlayer.PlayOnceAsync(_config.EmptyAmmoAudio.Random, transform.position, transform, true);
                }

                return;
            }

            Firing = true;
            ShootingStarted?.Invoke(this, EventArgs.Empty);
            FirstShotInLine = true;

            while (HeatNormalized < 1f)
            {
                while (_gamePauser.Paused == true) await UniTask.Yield();

                foreach (IGun gun in _activeGuns)
                {
                    if (token.IsCancellationRequested == true)
                    {
                        Firing = false;
                        ShootingStopped?.Invoke(this, EventArgs.Empty);

                        return;
                    }

                    ShotResult shotResult;

                    if (AuxMath.RandomNormal < _empFactor)
                    {
                        shotResult = await ActiveAmmo.FireAsync(user, gun);
                    }
                    else
                    {
                        await UniTask.WaitForSeconds(1f / gun.FireRate);
                        shotResult = ShotResult.None;
                    }

                    Heat = Mathf.Clamp(Heat + shotResult.Heat, 0f, HeatCapacity);

                    if (_config.ShakeOnShotFired == true) _masterCameraShaker.ShakeOnShotFired();

                    if (HeatNormalized == 1f)
                    {
                        await PerformOverheatAsync();
                        return;
                    }

                    if (FirstShotInLine == true) FirstShotInLine = false;
                }
            }
        }

        private async UniTask PerformOverheatAsync()
        {
            if (Overheat == true) return;

            Firing = false;
            ShootingStopped?.Invoke(this, EventArgs.Empty);

            Overheat = true;
            Overheated?.Invoke(this, EventArgs.Empty);

            if (_config.OverheatAudio == null)
            {
                float timer = 0f;

                while (timer < OverheatDuration)
                {
                    timer += Time.deltaTime;

                    while (_gamePauser.Paused == true) await UniTask.Yield();

                    await UniTask.Yield();
                }
            }
            else
            {
                await _audioPlayer.PlayOnceAsync(_config.OverheatAudio.Random, transform.position, transform, true);
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

            _userInventory.ContentChanged += async (sender, args) => await InventoryContentChangedEventHandlerAsync(sender, args);
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
            if (Firing == true) return false;
            if (_weaponsSwitchEnabled == false) return false;
            if (ActiveWeaponsSize == size) return false;

            _weaponsSwitchEnabled = false;

            List<IGun> activeGuns = new();

            foreach (IGun gun in _availableGuns)
                if (gun.AmmoSize == size)
                    activeGuns.Add(gun);

            List<AmmoSet> ammoSetsForActiveGuns = GetAmmo(size);

            if (activeGuns.Count > 0 && ammoSetsForActiveGuns.Count > 0)
            {
                AmmoSet previousAmmo = ActiveAmmo;

                _activeGuns = activeGuns;
                WeaponChanged?.Invoke(this, new(_activeGuns[0]));

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
                                await _audioPlayer.PlayOnceAsync(_config.SmallWeaponsSwitchAudio.Random, transform.position, transform, true);

                            break;
                        }
                    case Size.Medium:
                        {
                            if (_config.MediumWeaponsSwitchAudio == null)
                                await UniTask.WaitForSeconds(RandomMediumWeaponsSwitchDuration);
                            else
                                await _audioPlayer.PlayOnceAsync(_config.MediumWeaponsSwitchAudio.Random, transform.position, transform, true);

                            break;
                        }
                    case Size.Large:
                        {
                            if (_config.LargeWeaponsSwitchAudio == null)
                                await UniTask.WaitForSeconds(RandomLargeWeaponsSwitchDuration);
                            else
                                await _audioPlayer.PlayOnceAsync(_config.LargeWeaponsSwitchAudio.Random, transform.position, transform, true);

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
            else await _audioPlayer.PlayOnceAsync(_config.AmmoSwitchAudio.Random, transform.position, transform, true);

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
            else await _audioPlayer.PlayOnceAsync(_config.AmmoSwitchAudio.Random, transform.position, transform, true);

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

        private async UniTask InventoryContentChangedEventHandlerAsync(object sender, EventArgs e)
        {
            _availableAmmo = GetAvailableAmmo();
            _ammoForActiveWeapons = GetAmmo(ActiveWeaponsSize);

            if (Firing = false && ActiveAmmo.Amount == 0)
            {
                bool switchedFromDepletedAmmo = await TrySwitchToNextAmmoAsync();
                if (switchedFromDepletedAmmo == false) await TrySwitchToWorkingWeapons();
            }
        }

        #endregion

        #region EMP target interface

        private float _empFactor = 1f;
        private bool _empActive = false;

        public async UniTask<bool> TryApplyEMPAsync(EMP emp, CancellationToken token = default)
        {
            if (_empActive == true) return false;

            _empActive = true;

            float timer = 0f;

            while (timer < emp.Duration)
            {
                if (gameObject == null) return false;

                if (token.IsCancellationRequested == true ||
                    gameObject.activeInHierarchy == false)
                {
                    _empFactor = 1f;
                    _empActive = false;

                    return false;
                }

                if (_gamePauser.Paused == true) await UniTask.Yield();

                timer += Time.deltaTime;

                _empFactor = emp.GetCurrentFactor(timer);

                await UniTask.Yield();
            }

            _empFactor = 1f;
            _empActive = false;

            return true;
        }

        #endregion
    }
}