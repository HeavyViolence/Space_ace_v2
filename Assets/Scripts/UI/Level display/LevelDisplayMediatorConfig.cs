using SpaceAce.Auxiliary.Easing;

using System;

using UnityEngine;

namespace SpaceAce.UI
{
    [CreateAssetMenu(fileName = "Level display mediator config",
                     menuName = "Space ace/Configs/UI/Level display mediator config")]
    public sealed class LevelDisplayMediatorConfig : ScriptableObject
    {
        #region update duration

        public const float MinUpdateDuration = 0.1f;
        public const float MaxUpdateDuration = 10f;

        [SerializeField, Range(MinUpdateDuration, MaxUpdateDuration)]
        private float _updateDuration = MinUpdateDuration;

        public float UpdateDuration => _updateDuration;

        #endregion

        #region update easing

        [SerializeField]
        private EasingMode _updateEasing = EasingMode.Smooth;

        public EasingMode UpdateEasing => _updateEasing;

        #endregion

        #region bar update duration

        public const float MinBarUpdateDuration = 0.1f;
        public const float MaxBarUpdateDuration = 10f;

        [SerializeField, Range(MinBarUpdateDuration, MaxBarUpdateDuration), Space]
        private float _barUpdateDuration = MinBarUpdateDuration;

        public float BarUpdateDuration => _barUpdateDuration;

        #endregion

        #region bar update easing

        [SerializeField]
        private EasingMode _barUpdateEasing = EasingMode.Fast;

        public EasingMode BarUpdateEasing => _barUpdateEasing;

        #endregion

        #region heat gradient

        [SerializeField, Space]
        private Gradient _heatGradient;

        public Color GetHeatColor(float heatNormalized) =>
            _heatGradient.Evaluate(Mathf.Clamp01(heatNormalized));

        #endregion

        #region durability gradient

        [SerializeField]
        private Gradient _durabilityGradient;

        public Color GetDurabilityColor(float durabilityNormalized) =>
            _durabilityGradient.Evaluate(Mathf.Clamp01(durabilityNormalized));

        #endregion
    }
}