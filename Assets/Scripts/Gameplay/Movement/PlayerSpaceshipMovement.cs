using SpaceAce.Gameplay.Players;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    public sealed class PlayerSpaceshipMovement : Movement, IMovementController
    {
        private Vector2 _speed2D;
        private float _rotationSpeed;

        protected override void Awake()
        {
            base.Awake();

            _speed2D = new(NextHorizontalSpeed, NextVerticalSpeed);
            _rotationSpeed = NextRotationSpeed;
        }

        private Vector2 ClampMovementDirection(Vector2 rawDirection)
        {
            float x = rawDirection.x;
            float y = rawDirection.y;

            if (Body.position.x < LeftBound) x = Mathf.Clamp(x, 0f, 1f);
            if (Body.position.x > RightBound) x = Mathf.Clamp(x, -1f, 0f);
            if (Body.position.y > UpperBound) y = Mathf.Clamp(y, -1f, 0f);
            if (Body.position.y < LowerBound) y = Mathf.Clamp(y, 0f, 1f);

            return new Vector2(x, y);
        }

        #region interfaces

        public void Move(Vector2 direction)
        {
            Vector2 clampedDirection = ClampMovementDirection(direction);
            Vector2 velocity = clampedDirection * Time.fixedDeltaTime * _speed2D;

            Body.MovePosition(Body.position + velocity);
        }

        public void Rotate(Vector3 targetPosition)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, targetPosition);
            Quaternion currentRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);

            Body.MoveRotation(currentRotation);
        }

        #endregion
    }
}