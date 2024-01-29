using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    public sealed class MovementData
    {
        public static MovementData Default => new(0f, 0f, 0f, Vector3.zero, Vector3.zero, Quaternion.identity, null, 0f, 0f);

        public float Timer { get; set; }

        public float InitialSpeed { get; set; }
        public float CurrentSpeed { get; set; }
        public float FinalSpeed { get; set; }
        public float FinalSpeedGainDuration { get; set; }
            
        public Vector3 InitialPosition { get; }
        public Vector3 CurrentPosition { get; set; }

        public Vector3 InitialDirection { get; }
        public Vector3 CurrentDirection { get; set; }

        public Vector2 InitialVelocity => InitialDirection * InitialSpeed;
        public Vector2 CurrentVelocity => CurrentDirection * CurrentSpeed;

        public Quaternion InitialRotation { get; }
        public Quaternion CurrentRotation { get; set; }

        public Transform Target { get; set; }

        public float HomingSpeed { get; set; }
        public float RotationsPerMinute { get; set; }

        public MovementData(float initialSpeed,
                            float finalSpeed,
                            float finalSpeedGainDuration,
                            Vector3 initialPosition,
                            Vector3 initialDirection,
                            Quaternion initialRotation,
                            Transform target,
                            float homingSpeed,
                            float rotationsPerMinute)
        {
            Timer = 0f;

            InitialSpeed = Mathf.Clamp(initialSpeed, 0f, float.MaxValue);
            CurrentSpeed = InitialSpeed;
            FinalSpeed = Mathf.Clamp(finalSpeed, 0f, float.MaxValue);
            FinalSpeedGainDuration = Mathf.Clamp(finalSpeedGainDuration, 0f, float.MaxValue);

            InitialPosition = initialPosition;
            CurrentPosition = initialPosition;

            InitialDirection = initialDirection;
            CurrentDirection = initialDirection;

            InitialRotation = initialRotation;
            CurrentRotation = initialRotation;

            Target = target;
            HomingSpeed = Mathf.Clamp(homingSpeed, 0f, float.MaxValue);

            RotationsPerMinute = Mathf.Clamp(rotationsPerMinute, 0f, float.MaxValue);
        }
    }
}