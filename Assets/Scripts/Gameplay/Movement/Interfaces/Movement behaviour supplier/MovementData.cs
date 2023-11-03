using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    public sealed class MovementData
    {
        public float Timer { get; set; }

        public float InitialSpeed { get; private set; }
        public float CurrentSpeed { get; set; }
        public float FinalSpeed { get; private set; }
        public float FinalSpeedGainDuration { get; private set; }

        public Vector3 InitialPosition { get; }
        public Vector3 CurrentPosition { get; set; }

        public Vector3 InitialDirection { get; }
        public Vector3 CurrentDirection { get; set; }

        public Vector3 InitialVelocity => InitialDirection * InitialSpeed;
        public Vector3 CurrentVelocity => CurrentDirection * CurrentSpeed;

        public Quaternion InitialRotation {  get; }
        public Quaternion CurrentRotation { get; set; }

        public Transform Target { get; set; }

        public float HomingSpeed { get; private set; }
        public float RevolutionsPerMinute { get; private set; }

        public MovementData(float initialSpeed,
                            float finalSpeed,
                            float finalSpeedGainDuration,
                            Vector3 initialPosition,
                            Quaternion initialRotation,
                            Transform target,
                            float homingSpeed,
                            float revolutionsPerMinute)
        {
            Timer = 0f;

            InitialSpeed = Mathf.Abs(initialSpeed);
            CurrentSpeed = InitialSpeed;
            FinalSpeed = Mathf.Abs(finalSpeed);
            FinalSpeedGainDuration = Mathf.Abs(finalSpeedGainDuration);

            InitialPosition = initialPosition;
            CurrentPosition = initialPosition;

            InitialRotation = initialRotation;
            CurrentRotation = InitialRotation;

            Target = target;
            HomingSpeed = Mathf.Abs(homingSpeed);

            RevolutionsPerMinute = revolutionsPerMinute;
        }

        public static MovementData operator +(MovementData left, MovementData right)
        {
            if (left is null || right is null)
                throw new ArgumentNullException("Argument is empty!");

            MovementData result = new(left.InitialSpeed + right.InitialSpeed,
                                      left.FinalSpeed + right.FinalSpeed,
                                      left.FinalSpeedGainDuration + right.FinalSpeedGainDuration,
                                      Vector3.zero,
                                      Quaternion.identity,
                                      null,
                                      left.HomingSpeed + right.HomingSpeed,
                                      left.RevolutionsPerMinute + right.RevolutionsPerMinute);

            return result;
        }

        public static MovementData operator -(MovementData left, MovementData right)
        {
            if (left is null || right is null)
                throw new ArgumentNullException("Argument is empty!");

            MovementData result = new(left.InitialSpeed - right.InitialSpeed,
                                      left.FinalSpeed - right.FinalSpeed,
                                      left.FinalSpeedGainDuration - right.FinalSpeedGainDuration,
                                      Vector3.zero,
                                      Quaternion.identity,
                                      null,
                                      left.HomingSpeed - right.HomingSpeed,
                                      left.RevolutionsPerMinute - right.RevolutionsPerMinute);

            return result;
        }
    }
}