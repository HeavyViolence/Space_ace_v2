using System;

namespace SpaceAce.Gameplay.Movement
{
    public abstract class UncontrollableMovement : Movement, IMovementBehaviourSupplier
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
            _behaviour(Body, ref _data);
        }

        public void Supply(MovementBehaviour behaviour, MovementData data)
        {
            _behaviour = behaviour ?? throw new ArgumentNullException();
            _data = data ?? throw new ArgumentNullException();
        }
    }
}