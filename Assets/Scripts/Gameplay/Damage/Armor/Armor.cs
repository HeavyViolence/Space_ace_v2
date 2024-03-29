using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class Armor : MonoBehaviour, IEMPTarget
    {
        [SerializeField]
        private ArmorConfig _config;

        private GamePauser _gamePauser;

        public float Value { get; private set; }

        [Inject]
        private void Construct(GamePauser gamePauser)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
        }

        private void OnEnable()
        {
            Value = _config.RandomInitialValue;
        }

        public float GetReducedDamage(float damage)
        {
            if (damage < 0f) throw new ArgumentOutOfRangeException();
            return damage * damage / Value;
        }

        #region EMP target interface

        private float _empFactor = 1f;
        private bool _empActive = false;

        public async UniTask<bool> TryApplyEMPAsync(EMP emp, CancellationToken token = default)
        {
            if (_empActive == true) return false;

            _empActive = true;

            float initialValue = Value;
            float timer = 0f;

            while (timer < emp.Duration)
            {
                if (gameObject == null) return false;

                if (token.IsCancellationRequested == true ||
                    gameObject.activeInHierarchy == false)
                {
                    Value = initialValue;

                    _empFactor = 1f;
                    _empActive = false;

                    return false;
                }

                if (_gamePauser.Paused == true) await UniTask.Yield();

                timer += Time.deltaTime;

                _empFactor = emp.GetCurrentFactor(timer);
                Value = initialValue * _empFactor;

                await UniTask.Yield();
            }

            _empFactor = 1f;
            _empActive = false;

            return true;
        }

        #endregion
    }
}