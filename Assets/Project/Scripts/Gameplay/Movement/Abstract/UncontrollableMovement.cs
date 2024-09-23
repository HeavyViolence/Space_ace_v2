using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Effects;
using SpaceAce.Gameplay.Experience;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    public abstract class UncontrollableMovement : Movement, IMovementBehaviourSupplier, IExperienceSource, IStasisTarget
    {
        private MovementBehaviour _behaviour;
        private MovementData _data;

        protected override void OnEnable()
        {
            base.OnEnable();

            _behaviour = null;
            _data = null;
        }

        protected virtual void FixedUpdate()
        {
            if (_behaviour is null || _data is null || GamePauser.Paused == true) return;
            _behaviour(Body, _data);
        }

        #region interfaces

        public void Supply(MovementBehaviour behaviour, MovementData data)
        {
            _behaviour = behaviour ?? throw new ArgumentNullException();
            _data = data ?? throw new ArgumentNullException();
        }

        public float GetExperience() => _data.InitialSpeed;

        #endregion

        #region stasis target interface

        public bool StasisActive { get; private set; } = false;
        public float SpeedFactor { get; private set; } = 1f;

        public async UniTask<bool> TryApplyStasisAsync(Stasis stasis, CancellationToken token = default)
        {
            if (StasisActive == true) return false;

            StasisActive = true;

            float initialSpeed = _data.InitialSpeed;
            float currentSpeed = _data.CurrentSpeed;
            float timer = 0f;

            while (timer < stasis.Duration)
            {
                if (token.IsCancellationRequested == true || gameObject.activeInHierarchy == false) break;

                timer += Time.fixedDeltaTime;

                SpeedFactor = stasis.GetSpeedFactor(timer);

                _data.InitialSpeed = initialSpeed * SpeedFactor;
                _data.CurrentSpeed = currentSpeed * SpeedFactor;

                await UniTask.WaitUntil(() => GamePauser.Paused == false);
                await UniTask.WaitForFixedUpdate();
            }

            _data.InitialSpeed = initialSpeed;
            _data.CurrentSpeed = currentSpeed;

            StasisActive = false;
            SpeedFactor = 1f;

            return true;
        }

        #endregion
    }
}