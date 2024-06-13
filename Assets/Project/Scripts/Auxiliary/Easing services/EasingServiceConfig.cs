using UnityEngine;

namespace SpaceAce.Auxiliary.Easing
{
    [CreateAssetMenu(fileName = "Easing service config",
                     menuName = "Space ace/Configs/Auxiliary/Easing service config")]
    public sealed class EasingServiceConfig : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve _fast;

        public AnimationCurve Fast => _fast;

        [SerializeField]
        private AnimationCurve _slow;

        public AnimationCurve Slow => _slow;

        [SerializeField]
        private AnimationCurve _smooth;

        public AnimationCurve Smooth => _smooth;

        [SerializeField]
        private AnimationCurve _sharp;

        public AnimationCurve Sharp => _sharp;

        [SerializeField]
        private AnimationCurve _wavy;

        public AnimationCurve Wavy => _wavy;
    }
}