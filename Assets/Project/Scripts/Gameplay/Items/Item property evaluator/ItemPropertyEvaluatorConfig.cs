using UnityEngine;

namespace SpaceAce.Gameplay.Items
{
    [CreateAssetMenu(fileName = "Item property evaluator config",
                     menuName = "Space ace/Configs/Items/Item property evaluator config")]
    public sealed class ItemPropertyEvaluatorConfig : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve _goodPropertyInterpolationCurve;

        public AnimationCurve GoodPropertyInterpolationCurve => _goodPropertyInterpolationCurve;

        [SerializeField]
        private AnimationCurve _badPropertyInterpolationCurve;

        public AnimationCurve BadPropertyInterpolationCurve => _badPropertyInterpolationCurve;

        [SerializeField]
        private AnimationCurve _priceInterpolationCurve;

        public AnimationCurve PriceInterpolationCurve => _priceInterpolationCurve;

        [SerializeField]
        private AnimationCurve _valueProbabilityCurvePerRange;

        public AnimationCurve ValueProbabilityCurvePerRange => _valueProbabilityCurvePerRange;

        [SerializeField, Range(0f, 1f), Space]
        private float _smallSizePropertyFactor = 1f;

        public float SmallSizePropertyFactor => _smallSizePropertyFactor;

        [SerializeField, Range(1f, 2f)]
        private float _largeSizePropertyFactor = 1f;

        public float LargeSizePropertyFactor => _largeSizePropertyFactor;
    }
}