namespace SpaceAce.Gameplay.Movement
{
    public interface IMovementBehaviourSupplier
    {
        void Supply(MovementBehaviour behaviour, MovementData data);
    }
}