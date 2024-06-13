using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Homing ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Homing ammo set config")]
    public class HomingAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Homing;

        #region homing speed

        public const float MinHomingSpeed = 100f;
        public const float MaxHomingSpeed = 2000f;

        [SerializeField, MinMaxSlider(MinHomingSpeed, MaxHomingSpeed), Space]
        private Vector2 _homingSpeed = new(MinHomingSpeed, MaxHomingSpeed);

        public Vector2 HomingSpeed => _homingSpeed;

        #endregion

        #region targeting width

        public const float MinTargetingWidth = 2f;
        public const float MaxTargetingWidth = 20f;

        [SerializeField, MinMaxSlider(MinTargetingWidth, MaxTargetingWidth)]
        private Vector2 _targetingWidth = new(MinTargetingWidth, MaxTargetingWidth);

        public Vector2 TargetingWidth => _targetingWidth;

        #endregion
    }
}