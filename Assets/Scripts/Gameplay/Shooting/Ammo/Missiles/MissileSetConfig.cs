using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Missile set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Missile set config")]
    public class MissileSetConfig : HomingAmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Missiles;

        #region speed gain curve

        [SerializeField, Space]
        private AnimationCurve _speedGainCurve;

        public AnimationCurve SpeedGainCurve => _speedGainCurve;

        #endregion

        #region speed gain duration

        public const float MinSpeedGainDuration = 0.1f;
        public const float MaxSpeedGainDuration = 1f;

        [SerializeField, MinMaxSlider(MinSpeedGainDuration, MaxSpeedGainDuration)]
        private Vector2 _speedGainDuration = new(MinSpeedGainDuration, MaxSpeedGainDuration);

        public Vector2 SpeedGainDuration => _speedGainDuration;

        #endregion
    }
}