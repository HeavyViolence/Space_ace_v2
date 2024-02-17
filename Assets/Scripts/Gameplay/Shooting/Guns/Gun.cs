using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Shooting.Guns
{
    public sealed class Gun : MonoBehaviour, IEMPTarget
    {
        [SerializeField]
        private GunConfig _config;

        private GamePauser _gamePauser;

        public Size AmmoSize => _config.AmmoSize;
        public bool IsRightHanded => transform.localPosition.x > 0f;
        public float SignedConvergenceAngle => IsRightHanded ? -1f * _config.ConvergenceAngle : _config.ConvergenceAngle;
        public float FireRate => _config.FireRate * _empFactor;
        public float Dispersion => _config.Dispersion / _empFactor;

        [Inject]
        private void Construct(GamePauser gamePauser)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
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