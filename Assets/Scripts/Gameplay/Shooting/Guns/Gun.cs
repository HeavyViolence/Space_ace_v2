using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main;
using SpaceAce.Main.Localization;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Shooting.Guns
{
    public sealed class Gun : MonoBehaviour, IGun, IGunView, IEMPTarget
    {
        public event EventHandler ShootingStarted, ShootingStopped;
        public event EventHandler<ShotFiredEventArgs> ShotFired;

        [SerializeField]
        private GunConfig _config;

        private GamePauser _gamePauser;
        private Localizer _localizer;
        private MasterCameraShaker _masterCameraShaker;

        public Transform Transform { get; private set; }
        public Size AmmoSize => _config.AmmoSize;
        public bool FirstShotInLine { get; private set; } = true;
        public bool Firing { get; private set; }
        public bool IsRightHanded => Transform.localPosition.x > 0f;
        public float SignedConvergenceAngle => IsRightHanded ? -1f * _config.ConvergenceAngle : _config.ConvergenceAngle;
        public float FireRate => _config.FireRate * _empFactor;
        public float Dispersion => _config.Dispersion / _empFactor;

        [Inject]
        private void Construct(GamePauser gamePauser,
                               Localizer localzier,
                               MasterCameraShaker masterCameraShaker)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _localizer = localzier ?? throw new ArgumentNullException();
            _masterCameraShaker = masterCameraShaker ?? throw new ArgumentNullException();
        }

        private void Awake()
        {
            Transform = transform;
        }

        public async UniTask<string> GetSizeCodeAsync() =>
            await _localizer.GetLocalizedStringAsync("Guns", "Size code", this);

        public async UniTask FireAsync(object shooter,
                                       AmmoSet ammo,
                                       CancellationToken fireCancellation = default,
                                       CancellationToken overheatCancellation = default)
        {
            if (shooter is null) throw new ArgumentNullException();
            if (ammo is null) throw new ArgumentNullException();

            if (Firing == true) return;

            Firing = true;
            ShootingStarted?.Invoke(this, EventArgs.Empty);
            FirstShotInLine = true;

            while (fireCancellation.IsCancellationRequested == false &&
                   overheatCancellation.IsCancellationRequested == false)
            {
                if (AuxMath.RandomNormal < _empFactor)
                {
                    ShotResult result = ammo.ShotBehaviour(shooter, this);
                    ShotFired?.Invoke(this, new(result));

                    if (FirstShotInLine == true) FirstShotInLine = false;
                    if (_config.ShakeOnShotFired == true) _masterCameraShaker.ShakeOnShotFired();
                }

                await UniTask.WaitUntil(() => _gamePauser.Paused == false);
                await UniTask.WaitForSeconds(1f / FireRate);
            }

            Firing = false;
            ShootingStopped?.Invoke(this, EventArgs.Empty);
        }

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
                if (token.IsCancellationRequested == true || gameObject.activeInHierarchy == false) break;

                timer += Time.deltaTime;

                _empFactor = emp.GetCurrentFactor(timer);

                await UniTask.WaitUntil(() => _gamePauser.Paused == false);
                await UniTask.Yield();
            }

            _empFactor = 1f;
            _empActive = false;

            return true;
        }

        #endregion
    }
}