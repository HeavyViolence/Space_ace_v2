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
        public const float MaxUpdateDuration = 3f;

        [SerializeField, Range(MinUpdateDuration, MaxUpdateDuration)]
        private float _updateDuration = MinUpdateDuration;

        public float UpdateDuration => _updateDuration;

        #endregion

        #region bar update duration

        public const float MinBarUpdateDuration = 0.1f;
        public const float MaxBarUpdateDuration = 1f;

        [SerializeField, Range(MinBarUpdateDuration, MaxBarUpdateDuration)]
        private float _barUpdateDuration = MinBarUpdateDuration;

        public float BarUpdateDuration => _barUpdateDuration;

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