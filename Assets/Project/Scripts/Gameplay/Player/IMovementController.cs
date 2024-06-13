using UnityEngine;

namespace SpaceAce.Gameplay.Players
{
    public interface IMovementController
    {
        void Move(Vector2 direction);
        void Rotate(Vector3 targetPosition);
    }
}