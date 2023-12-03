namespace SpaceAce.Gameplay.Movement
{
    public interface IMovementBehaviourSupplier
    {
        void SupplyMovementBehaviour(MovementBehaviour behaviour, MovementData data);
    }
}