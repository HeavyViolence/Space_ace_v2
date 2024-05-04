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
        public Size AmmoSize => _config.AmmoSize;
        public bool IsRightHanded => transform.localPosition.x > 0f;
        public float SignedConvergenceAngle => IsRightHanded ? -1f * _config.ConvergenceAngle : _config.ConvergenceAngle;
        public float FireRate => _config.FireRate * _empFactor;
        public float Dispersion => _config.Dispersion / _empFactor;

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
                if (token.IsCancellationRequested == true || gameObject.activeInHierarchy == false)
                {
                    _empFactor = 1f;
                    _empActive = false;

                    return false;
                }

                timer += Time.deltaTime;

                _empFactor = emp.GetCurrentFactor(timer);

                await UniTask.WaitUntil(() => _gamePauser.Paused == false, PlayerLoopTiming.Update, token);
                await UniTask.Yield();
            }

            _empFactor = 1f;
            _empActive = false;

            return true;
        }

        #endregion
    }
}