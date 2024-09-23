using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Effects;
using SpaceAce.Gameplay.Players;

using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    public sealed class ShipMovement : ControllableMovement, IMovementController, IStasisTarget
    {
        [SerializeField]
        private bool _invertAllowedRotation = false;

        private Vector2 _maxVelocity;
        public Vector2 MaxVelocity
        {
            private set => _maxVelocity = value;
            get => _maxVelocity * SpeedFactor;
        }

        private float _movementStiffness;
        public float MovementStiffness
        {
            private set => _movementStiffness = value;
            get => SpeedFactor == 0f ? 0f : _movementStiffness * SpeedFactor;
        }

        private float _brakingStiffness;
        public float BrakingStiffness
        {
            private set => _brakingStiffness = value;
            get => SpeedFactor == 0f ? 0f : _brakingStiffness * SpeedFactor;
        }

        private float _viewportReboundStiffness;
        public float ViewportReboundStiffness
        {
            private set => _viewportReboundStiffness = value;
            get => SpeedFactor == 0f ? 0f : _viewportReboundStiffness * SpeedFactor;
        }

        private float _rotationStiffness;
        public float RotationStiffness
        {
            private set => _rotationStiffness = value;
            get => SpeedFactor == 0f ? 0f : _rotationStiffness * SpeedFactor;
        }

        private float _maxVelocityMagnitude;
        private Vector2 _currentVelocity;

        private CancellationTokenSource _waitUntilInsideViewportCancellation;
        private bool _insideViewport;

        protected override void OnEnable()
        {
            base.OnEnable();

            _currentVelocity = Vector2.zero;
            MaxVelocity = new(NextHorizontalSpeed, NextVerticalSpeed);
            _maxVelocityMagnitude = MaxVelocity.magnitude;

            MovementStiffness = NextMovementStiffness;
            BrakingStiffness = NextBrakingSmoothness;
            ViewportReboundStiffness = NextViewportReboundStiffness;
            RotationStiffness = NextRotationStiffness;

            _waitUntilInsideViewportCancellation = new();
            WaitUntilInsideViewportAsync(_waitUntilInsideViewportCancellation.Token).Forget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _waitUntilInsideViewportCancellation?.Cancel();
            _waitUntilInsideViewportCancellation?.Dispose();
        }

        private void FixedUpdate()
        {
            ReboundMovementDirectionNearViewport();
        }

        #region interfaces

        public void Move(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                _currentVelocity = Vector2.Lerp(_currentVelocity, Vector2.zero, Time.deltaTime * BrakingStiffness);
            }
            else
            {
                _currentVelocity = Vector2.Lerp(_currentVelocity, MaxVelocity * direction, Time.deltaTime * MovementStiffness);
            }

            _currentVelocity = Vector2.ClampMagnitude(_currentVelocity, _maxVelocityMagnitude * Time.fixedDeltaTime);
            Body.MovePosition(Body.position + _currentVelocity);
        }

        public void Rotate(Vector3 targetPosition)
        {
            float clampedY = _invertAllowedRotation == true ? Mathf.Clamp(targetPosition.y, float.NegativeInfinity, 0f)
                                                            : Mathf.Clamp(targetPosition.y, 0f, float.PositiveInfinity);

            Vector3 clampedMouseWorldPosition = new(targetPosition.x, clampedY, targetPosition.z);
            Vector3 clampedMouseDirection = Transform.position - clampedMouseWorldPosition;

            Quaternion clampedMouseRotation = Quaternion.LookRotation(clampedMouseDirection, Vector3.forward);
            Quaternion interpolatedMouseRotation = Quaternion.Lerp(Transform.rotation, clampedMouseRotation, Time.fixedDeltaTime * RotationStiffness);

            Body.MoveRotation(interpolatedMouseRotation);
        }

        #endregion

        private void ReboundMovementDirectionNearViewport()
        {
            if (_insideViewport == false || GamePauser.Paused == true) return;

            if (Body.position.x < ModifiedLeftBound)
                Move(Vector2.right * ViewportReboundStiffness);

            if (Body.position.x > ModifiedRightBound)
                Move(Vector2.left * ViewportReboundStiffness);

            if (Body.position.y < ModifiedLowerBound)
                Move(Vector2.up * ViewportReboundStiffness);

            if (Body.position.y > ModifiedUpperBound)
                Move(Vector2.down * ViewportReboundStiffness);
        }

        private async UniTask WaitUntilInsideViewportAsync(CancellationToken token)
        {
            _insideViewport = false;

            await AuxAsync.DelayAsync(() => MasterCameraHolder.InsideViewport(Transform.position) == false,
                                      () => GamePauser.Paused == true, token);

            if (token.IsCancellationRequested == true) return;

            _insideViewport = true;
        }

        #region stasis target interface

        public bool StasisActive { get; private set; } = false;
        public float SpeedFactor { get; private set; } = 1f;

        public async UniTask<bool> TryApplyStasisAsync(Stasis stasis, CancellationToken token = default)
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