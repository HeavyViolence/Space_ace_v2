using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    [CreateAssetMenu(fileName = "Spaceship movement config",
                     menuName = "Space ace/Configs/Movement/Spaceship movement config")]
    public sealed class SpaceshipMovementConfig : MovementConfig
    {
        public const float MinSpeedDuration = 0.5f;
        public const float MaxSpeedDuration = 5f;

        public const float MinSpeedTransitionDuration = 0.25f;
        public const float MaxSpeedTransitionDuration = 5f;

        public const float MinPlayerAimSpeed = 0f;
        public const float MaxPlayerAimSpeed = 30f;

        [SerializeField, MinMaxSlider(MinSpeedDuration, MaxSpeedDuration), HorizontalLine]
        private Vector2 _horizontalSpeedDuration = new(MinSpeedDuration, MaxSpeedDuration);

        [SerializeField, MinMaxSlider(MinSpeedTransitionDuration, MaxSpeedTransitionDuration)]
        private Vector2 _horizontalSpeedTransitionDuration = new(MinSpeedTransitionDuration, MaxSpeedTransitionDuration);

        [SerializeField, MinMaxSlider(MinSpeedDuration, MaxSpeedDuration)]
        private Vector2 _verticalsSpeedDuration = new(MinSpeedDuration, MaxSpeedDuration);

        [SerializeField, MinMaxSlider(MinSpeedTransitionDuration, MaxSpeedTransitionDuration)]
        private Vector2 _verticalSpeedTransitionDuration = new(MinSpeedTransitionDuration, MaxSpeedTransitionDuration);

        [SerializeField, MinMaxSlider(MinPlayerAimSpeed, MaxPlayerAimSpeed), HorizontalLine]
        private Vector2 _playerAimSpeed = new(MinPlayerAimSpeed, MaxPlayerAimSpeed);

        public float MinHorizontalSpeedDuration => _horizontalSpeedDuration.x;
        public float MaxHorizontalSpeedDuration => _horizontalSpeedDuration.y;

        public float MinHorizontalSpeedTransitionDuration => _horizontalSpeedTransitionDuration.x;
        public float MaxHorizontalSpeedTransitionDuration => _horizontalSpeedTransitionDuration.y;

        public float MinVerticalSpeedDuration => _verticalsSpeedDuration.x;
        public float MaxVerticalSpeedDuration => _verticalsSpeedDuration.y;

        public float MinVerticalSpeedTransitionDuration => _verticalSpeedTransitionDuration.x;
        public float MaxVerticalSpeedTransitionDuration => _verticalSpeedTransitionDuration.y;

        public float LowestPlayerAimSpeed => _playerAimSpeed.x;
        public float HighestPlayerAimSpeed => _playerAimSpeed.y;
    }
}