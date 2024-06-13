using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    public delegate void MovementBehaviour(Rigidbody2D body, ref MovementData data);
}