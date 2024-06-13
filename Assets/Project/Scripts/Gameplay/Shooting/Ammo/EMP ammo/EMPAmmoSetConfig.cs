using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "EMP ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/EMP ammo set config")]
    public sealed class EMPAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.EMP;

        #region EMP strength

        public const float MinEMPStrength = 0f;
        public const float MaxEMPStrength = 1f;

        [SerializeField, MinMaxSlider(MinEMPStrength, MaxEMPStrength), Space]
        private Vector2 _empStrength = new(MinEMPStrength, MaxEMPStrength);

        public Vector2 EMPStrength => _empStrength;

        #endregion

        #region EMP duration

        public const float MinEMPDuration = 1f;
        public const float MaxEMPDuration = 20f;

        [SerializeField, MinMaxSlider(MinEMPDuration, MaxEMPDuration)]
        private Vector2 _empDuration = new(MinEMPDuration, MaxEMPDuration);

        public Vector2 EMPDuration => _empDuration;

        #endregion

        #region EMP strength over time

        [SerializeField, Space]
        private AnimationCurve _empStrenghtOverTime;

        public AnimationCurve EMPStrenghtOverTime => _empStrenghtOverTime;

        #endregion
    }
}