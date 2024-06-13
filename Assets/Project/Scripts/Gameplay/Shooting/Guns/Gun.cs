using Cysharp.Threading.Tasks;

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
    public sealed class Gun : MonoBehaviour, IGun, IEMPTarget
    {
        [SerializeField]
        private GunConfig _config;

        private GamePauser _gamePauser;
        private Localizer _localizer;

        public Transform Transform { get; private set; }
        public Size Size => _config.Size;
        public bool Firing { get; private set; }
        public bool IsRightHanded => Transform.localPosition.x > 0f;
        public bool ShakeOnShotFired => _config.ShakeOnShotFired;
        public float SignedConvergenceAngle => IsRightHanded ? -1f * _config.ConvergenceAngle : _config.ConvergenceAngle;
        public float FireRate => _config.FireRate * (1f - EMPFactor);
        public float Dispersion
        {
            get
            {
                if (EMPFactor == 0f) return _config.Dispersion;
                if (EMPFactor == 1f) return GunConfig.MaxDispersion;

                return Mathf.Clamp(_config.Dispersion / (1f - EMPFactor), 0f, GunConfig.MaxDispersion);
            }
        }

        [Inject]
        private void Construct(GamePauser gamePauser, Localizer localzier)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _localizer = localzier ?? throw new ArgumentNullException();
        }

        private void Awake()
        {
            Transform = transform;
        }

        public async UniTask<string> GetSizeCodeAsync() =>
            await _localizer.GetLocalizedStringAsync("Guns", "Size code", this);

        public async UniTask<string> GetNoGunsCodeAsync() =>
            await _localizer.GetLocalizedStringAsync("Guns", "None", this);

        #region EMP target interface

        public bool EMPActive { get; private set; } = false;
        public float EMPFactor { get; private set; } = 0f;

        public async UniTask<bool> TryApplyEMPAsync(EMP emp, CancellationToken token = default)
        {
            if (EMPActive == true) return false;

            EMPActive = true;
            float timer = 0f;

            while (timer < emp.Duration)
            {
                if (token.IsCancellationRequested == true || gameObject.activeInHierarchy == false) break;

                timer += Time.deltaTime;

                EMPFactor = emp.GetFactor(timer);

                await UniTask.WaitUntil(() => _gamePauser.Paused == false);
                await UniTask.Yield();
            }

            EMPFactor = 0f;
            EMPActive = false;

            return true;
        }

        #endregion
    }
}