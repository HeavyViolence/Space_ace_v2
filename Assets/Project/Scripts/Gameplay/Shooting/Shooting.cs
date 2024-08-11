using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Experience;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Localization;
using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Effects;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Shooting
{
    public sealed class Shooting : MonoBehaviour, IShooterView, IExperienceSource, IEMPTargetProvider
    {
        public event EventHandler ShootingStarted, ShootingStopped;
        public event EventHandler<FloatValueChangedEventArgs> HeatValueChanged, HeatCapacityChanged;
        public event EventHandler Overheated, CooledDown;
        public event EventHandler GunsSwitched;

        [SerializeField]
        private ShootingConfig _config;

        private readonly List<Gun> _availableGuns = new();
        private readonly List<Gun> _activeGuns = new();

        private readonly List<AmmoSet> _availableAmmo = new();
        private readonly List<AmmoSet> _activeGunsAmmo = new();
        private AmmoSet _activeAmmo;

        private float _baseHeatLossRate;

        private GamePauser _gamePauser;
        private AudioPlayer _audioPlayer;
        private Localizer _localizer;
        private Inventory _inventory;
        private Transform _transform;

        private bool _gunsSwitchEnabled = true;
        private bool _firstGunsSwitch = true;

        public float MinInitialHeatCapacity => _config.MinInitialHeatCapacity;
        public float MaxInitialHeatCapacity => _config.MaxInitialHeatCapacity;
        public float RandomInitialHeatCapacity => _config.RandomInitialHeatCapacity;

        public float MinInitialBaseHeatLossRate => _config.MinInitialBaseHeatLossRate;
        public float MaxInitialBaseHeatLossRate => _config.MaxInitialBaseHeatLossRate;
        public float RandomInitialBaseHeatLossRate => _config.RandomInitialBaseHeatLossRate;

        private CancellationTokenSource _overheatCancellation;

        private float _heat;
        public float Heat
        {
            get => _heat;

            private set
            {
                float oldValue = _heat;
                float newValue = Mathf.Clamp(value, 0f, HeatCapacity);

                _heat = newValue;
                HeatValueChanged?.Invoke(this, new(oldValue, newValue));

                if (HeatNormalized == 1f) PerformOverheatAsync().Forget();
            }
        }

        private float _heatCapacity;
        public float HeatCapacity
        {
            get => _heatCapacity;

            private set
            {
                float oldValue = _heatCapacity;
                float newValue = Mathf.Clamp(value, 0f, ShootingConfig.MaxHeatCapacity);

                _heatCapacity = newValue;
                HeatCapacityChanged?.Invoke(this, new(oldValue, newValue));
            }
        }

        public float HeatNormalized => Heat / HeatCapacity;
        public float HeatPercentage => HeatNormalized * 100f;
        public float OverheatDuration { get; private set; }

        public bool Overheat { get; private set; }
        public bool GunsSelected => _activeGuns is not null && _activeGuns.Count > 0;
        public bool Firing => GunsSelected == true && _activeGuns[0].Firing;

        public Size ActiveGunsSize { get; private set; }

        public int ActiveAmmoIndex { get; private set; }
        public int ActiveGunsAmmoCount => _activeGunsAmmo.Count;

        public AmmoView ActiveAmmoView { get; private set; }

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
            _availableGuns.AddRange(FindAllGuns());
            ActiveAmmoView = new(_localizer, null);
        }

        private void OnEnable()
        {
            _firstGunsSwitch = true;

            _activeAmmo = null;
            ActiveAmmoIndex = -1;

            HeatCapacity = RandomInitialHeatCapacity;
            Heat = 0f;
            _baseHeatLossRate = RandomInitialBaseHeatLossRate;
            OverheatDuration = _config.OverheatDuration;
            Overheat = false;
        }

        private void OnDisable()
        {
            _inventory.ContentChanged -= async (_, _) => await UpdateAvailableAmmoAsync();
            _inventory = null;

            ActiveGunsSize = (Size)int.MinValue;

            _empTargets.Clear();
        }

        private void Update()
        {
            if (_gamePauser.Paused == true || Heat == 0f || Overheat == true) return;
            Heat = Mathf.Clamp(Heat - GetHeatLossThisFrame(), 0f, HeatCapacity);
        }

        private IEnumerable<Gun> FindAllGuns()
        {
            Gun[] guns = gameObject.GetComponentsInChildren<Gun>();

            if (guns.Length == 0) throw new Exception($"There are no guns on the object!");
            return guns;
        }

        private float GetHeatLossThisFrame()
        {
            float factor = _config.HeatLossFactorCurve.Evaluate(HeatNormalized);
            return _baseHeatLossRate * factor * Time.deltaTime;
        }

        public float GetDamagePerSecond()
        {
            if (_activeGuns is null || _activeAmmo is null) return 0f;

            float damagePerSecond = 0f;

            foreach (Gun gun in _activeGuns)
                damagePerSecond += gun.FireRate * _activeAmmo.Damage;

            return damagePerSecond;
        }

        public async UniTask FireAsync(object shooter, CancellationToken token)
        {
            if (shooter is null) throw new ArgumentNullException();

            if (Firing == true || Overheat == true) return;

            if (_activeAmmo is null)
            {
                _audioPlayer.PlayOnceAsync(_config.EmptyAmmoAudio.Random, _transform.position, _transform, true).Forget();
                return;
            }

            _overheatCancellation = new();
            CancellationTokenSource overheatOrCancelled = CancellationTokenSource.CreateLinkedTokenSource(_overheatCancellation.Token, token);

            ShootingStarted?.Invoke(this, EventArgs.Empty);
            _activeAmmo.ShotFired += (_, e) => Heat += e.Heat;

            foreach (Gun gun in _activeGuns)
                _activeAmmo.FireAsync(shooter, gun, overheatOrCancelled.Token).Forget();
            
            await UniTask.WaitUntil(() => _gamePauser.Paused == false);

            ShootingStopped?.Invoke(this, EventArgs.Empty);
            _activeAmmo.ShotFired -= (_, e) => Heat += e.Heat;
        }

        private async UniTask PerformOverheatAsync()
        {
            if (Overheat == true) return;

            ShootingStopped?.Invoke(this, EventArgs.Empty);

            Overheat = true;
            Overheated?.Invoke(this, EventArgs.Empty);

            _overheatCancellation?.Cancel();
            _overheatCancellation?.Dispose();

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

        public async UniTask BindInventoryAsync(Inventory inventory)
        {
            _inventory = inventory ?? throw new ArgumentNullException();

            if (TryGetAvailableAmmo(out IEnumerable<AmmoSet> ammo) == true)
            {
                _availableAmmo.Clear();
                _availableAmmo.AddRange(ammo);

                await TrySwitchToWorkingGunsAsync();
            }

            _inventory.ContentChanged += async (_, _) => await UpdateAvailableAmmoAsync();
        }

        private async UniTask<bool> TrySwitchToWorkingGunsAsync()
        {
            IEnumerable<Size> gunSizes = Enum.GetValues(typeof(Size)).Cast<Size>();

            foreach (Size size in gunSizes)
                if (await TrySwitchToGunsAsync(size) == true)
                    return true;

            return false;
        }

        public async UniTask<bool> TrySwitchToGunsAsync(Size size)
        {
            if (Firing == true || _gunsSwitchEnabled == false || ActiveGunsSize == size) return false;

            _gunsSwitchEnabled = false;

            if (TryGetGuns(size, out IEnumerable<Gun> guns) == true)
            {
                _activeGuns.Clear();
                _activeGuns.AddRange(guns);

                if (TryGetFittingAmmo(size, out IEnumerable<AmmoSet> ammo) == true)
                {
                    await PerformGunsSwitchDelayAsync(size);

                    _activeGunsAmmo.Clear();
                    _activeGunsAmmo.AddRange(ammo);

                    ActiveAmmoIndex = 0;
                    _activeAmmo = _activeGunsAmmo[0];
                    ActiveGunsSize = size;
                    _gunsSwitchEnabled = true;

                    ActiveAmmoView.Rebind(_activeAmmo);
                    GunsSwitched?.Invoke(this, EventArgs.Empty);

                    return true;
                }
                else
                {
                    ActiveAmmoView.Reset();
                    _gunsSwitchEnabled = true;

                    return false;
                }
            }
            else
            {
                _gunsSwitchEnabled = true;
                return false;
            }
        }

        private async UniTask PerformGunsSwitchDelayAsync(Size size)
        {
            if (_firstGunsSwitch == true)
            {
                if (_config.FirstGunsSwitchAudioEnabled == true)
                    await _audioPlayer.PlayOnceAsync(_config.GetGunsSwitchAudio(size).Random, _transform.position, _transform, true);
                else
                    await UniTask.WaitForSeconds(_config.GetGunsSwitchDuration(size));
            }
            else
            {
                await _audioPlayer.PlayOnceAsync(_config.GetGunsSwitchAudio(size).Random, _transform.position, _transform, true);
            }

            if (_firstGunsSwitch == true) _firstGunsSwitch = false;
        }

        private bool TryGetGuns(Size size, out IEnumerable<Gun> guns)
        {
            List<Gun> result = new();

            foreach (Gun gun in _availableGuns)
                if (gun.Size == size)
                    result.Add(gun);

            if (result.Count == 0)
            {
                guns = null;
                return false;
            }
            else
            {
                guns = result;
                return true;
            }
        }

        private bool TryGetAvailableAmmo(out IEnumerable<AmmoSet> ammo)
        {
            if (_inventory is null) throw new ArgumentNullException();

            List<AmmoSet> availableAmmo = new();

            foreach (IItem item in _inventory.GetItems())
                if (item is AmmoSet set)
                    availableAmmo.Add(set);

            if (availableAmmo.Count == 0)
            {
                ammo = null;
                return false;
            }
            else
            {
                ammo = availableAmmo;
                return true;
            }
        }

        private bool TryGetFittingAmmo(Size size, out IEnumerable<AmmoSet> ammo)
        {
            if (_availableAmmo is null) throw new ArgumentNullException();

            List<AmmoSet> fittingAmmo = new();

            foreach (AmmoSet set in _availableAmmo)
                if (set.Size == size)
                    fittingAmmo.Add(set);

            if (fittingAmmo.Count == 0)
            {
                ammo = null;
                return false;
            }
            else
            {
                ammo = fittingAmmo;
                return true;
            }
        }

        public async UniTask<bool> TrySwitchToNextAmmoAsync()
        {
            if (Firing == true) return false;

            if (ActiveGunsAmmoCount <= 1)
            {
                await _audioPlayer.PlayOnceAsync(_config.EmptyAmmoAudio.Random, _transform.position, _transform, true);
                return false;
            }

            if (_config.AmmoSwitchAudio == null) await UniTask.WaitForSeconds(_config.AmmoSwitchDuration);
            else await _audioPlayer.PlayOnceAsync(_config.AmmoSwitchAudio.Random, _transform.position, _transform, true);

            ActiveAmmoIndex = ActiveAmmoIndex++ % ActiveGunsAmmoCount;
            _activeAmmo = _activeGunsAmmo[ActiveAmmoIndex];
            ActiveAmmoView.Rebind(_activeAmmo);

            return true;
        }

        public async UniTask<bool> TrySwitchToPreviousAmmoAsync()
        {
            if (Firing == true) return false;

            if (ActiveGunsAmmoCount <= 1)
            {
                await _audioPlayer.PlayOnceAsync(_config.EmptyAmmoAudio.Random, _transform.position, _transform, true);
                return false;
            }

            if (_config.AmmoSwitchAudio == null) await UniTask.WaitForSeconds(_config.AmmoSwitchDuration);
            else await _audioPlayer.PlayOnceAsync(_config.AmmoSwitchAudio.Random, _transform.position, _transform, true);

            if (ActiveAmmoIndex == 0) ActiveAmmoIndex = ActiveGunsAmmoCount - 1;
            else ActiveAmmoIndex--;

            _activeAmmo = _activeGunsAmmo[ActiveAmmoIndex];
            ActiveAmmoView.Rebind(_activeAmmo);

            return true;
        }

        #region event handlers

        private async UniTask UpdateAvailableAmmoAsync()
        {
            if (TryGetAvailableAmmo(out IEnumerable<AmmoSet> availableAmmo) == true)
            {
                _availableAmmo.Clear();
                _availableAmmo.AddRange(availableAmmo);
            }

            if (TryGetFittingAmmo(ActiveGunsSize, out IEnumerable<AmmoSet> fittingAmmo) == true)
            {
                _activeGunsAmmo.Clear();
                _activeGunsAmmo.AddRange(fittingAmmo);
            }

            if (Firing == false && _activeAmmo.Amount == 0)
            {
                bool switchedFromDepletedAmmo = await TrySwitchToNextAmmoAsync();
                if (switchedFromDepletedAmmo == false) await TrySwitchToWorkingGunsAsync();
            }
        }

        #endregion

        public float GetExperience() => GetDamagePerSecond();

        #region EMP target interface

        private List<IEMPTarget> _empTargets = new();

        public IEnumerable<IEMPTarget> GetTargets()
        {
            if (_empTargets.Count != _availableGuns.Count + _availableAmmo.Count)
            {
                _empTargets = new(_availableGuns.Count + _availableAmmo.Count);
                _empTargets.AddRange(_availableGuns);
                _empTargets.AddRange(_availableAmmo);
            }

            return _empTargets;
        }

        #endregion
    }
}