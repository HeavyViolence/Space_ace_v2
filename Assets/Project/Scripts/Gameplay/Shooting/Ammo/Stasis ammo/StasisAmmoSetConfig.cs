using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Stasis ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Stasis ammo set config")]
    public sealed class StasisAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Stasis;

        #region slowdown

        public const float MinSlowdownFactor = 0f;
        public const float MaxSlowdownFactor = 1f;

        [SerializeField, MinMaxSlider(MinSlowdownFactor, MaxSlowdownFactor), Space]
        private Vector2 _slowdown = new(MinSlowdownFactor, MaxSlowdownFactor);

        public Vector2 Slowdown => _slowdown;

        #endregion

        #region slowdown over time

        [SerializeField]
        private AnimationCurve _slowdownOverTime;

        public AnimationCurve SlowdownOverTime => _slowdownOverTime;

        #endregion

        #region slowdown duration

        public const float MinSlowdownDuration = 0f;
        public const float MaxSlowdownDuration = 20f;

        [SerializeField, MinMaxSlider(MinSlowdownDuration, MaxSlowdownDuration)]
        private Vector2 _slowdownDuration = new(MinSlowdownDuration, MaxSlowdownDuration);

        public Vector2 SlowdownDuration => _slowdownDuration;

        #endregion
    }
}