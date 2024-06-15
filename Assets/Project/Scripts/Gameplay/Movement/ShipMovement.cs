using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Effects;
using SpaceAce.Gameplay.Players;

using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    public sealed class ShipMovement : ControllableMovement, IMovementController, IStasisTarget
    {
        private Vector2 _velocity;
        private Vector2 Velocity
        {
            set => _velocity = value;
            get => _velocity * SpeedFactor;
        }

        private float _rotationSpeed;

        protected override void Awake()
        {
            base.Awake();

            Velocity = new(MaxHorizontalSpeed, MaxVerticalSpeed);
            _rotationSpeed = MaxRotationSpeed;
        }

        #region interfaces

        public void Move(Vector2 direction)
        {
            Vector2 clampedDirection = ClampMovementDirection(direction);
            Vector2 velocity = Velocity * clampedDirection * Time.fixedDeltaTime;

            Body.MovePosition(Body.position + velocity);
        }

        public void Rotate(Vector3 targetPosition)
        {
            float clampedY = Mathf.Clamp(targetPosition.y, 0f, float.PositiveInfinity);

            Vector3 clampedMouseWorldPosition = new(targetPosition.x, clampedY, targetPosition.z);
            Vector3 clampedMouseDirection = transform.position - clampedMouseWorldPosition;

            Quaternion clampedMouseRotation = Quaternion.LookRotation(clampedMouseDirection, Vector3.forward);
            Quaternion interpolatedMouseRotation = Quaternion.Lerp(Transform.rotation, clampedMouseRotation, Time.fixedDeltaTime * _rotationSpeed);

            Body.MoveRotation(interpolatedMouseRotation);
        }

        #endregion

        private Vector2 ClampMovementDirection(Vector2 rawDirection)
        {
            float x = rawDirection.x;
            float y = rawDirection.y;

            if (Body.position.x < LeftBound) x = Mathf.Clamp(x, 0f, 1f);
            if (Body.position.x > RightBound) x = Mathf.Clamp(x, -1f, 0f);
            if (Body.position.y > UpperBound) y = Mathf.Clamp(y, -1f, 0f);
            if (Body.position.y < LowerBound) y = Mathf.Clamp(y, 0f, 1f);

            return new(x, y);
        }

        #region stasis target interface

        public bool StasisActive { get; private set; } = false;
        public float SpeedFactor { get; private set; } = 1f;

        public async UniTask<bool> TryApplyStasis(Stasis stasis, CancellationToken token = default)
        {
            if (StasisActive == true) return false;

            StasisActive = true;
            float timer = 0f;

            while (timer < stasis.Duration)
            {
                if (token.IsCancellationRequested == true || gameObject.activeInHierarchy == false) break;

                timer += Time.fixedDeltaTime;

                SpeedFactor = stasis.GetSpeedFactor(timer);

                await UniTask.WaitUntil(() => GamePauser.Paused == false);
                await UniTask.WaitForFixedUpdate();
            }

            StasisActive = false;
            SpeedFactor = 1f;

            return true;
        }

        #endregion
    }
}