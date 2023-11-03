namespace SpaceAce.Gameplay.Movement
{
    public interface IMovementBehaviourSupplier
    {
        public void SupplyMovementBehaviour(MovementBehaviour behaviour, MovementData data);
    }
}