using UnityEngine;

namespace SpaceAce.UI.Displays
{
    [CreateAssetMenu(fileName = "Level display config",
                     menuName = "Space ace/Configs/UI/Level display config")]
    public sealed class LevelDisplayConfig : ScriptableObject
    {
        #region target display duration

        public const float MinTargetDisplayDuration = 0f;
        public const float MaxTargetDisplayDuration = 10f;

        [SerializeField, Range(MinTargetDisplayDuration, MaxTargetDisplayDuration)]
        private float _targetDisplayDuration = MinTargetDisplayDuration;

        public float TargetDisplayDuration => _targetDisplayDuration;

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