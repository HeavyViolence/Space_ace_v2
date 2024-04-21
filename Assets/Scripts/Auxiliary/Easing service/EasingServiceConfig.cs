using UnityEngine;

namespace SpaceAce.Auxiliary
{
    [CreateAssetMenu(fileName = "Easing service config",
                     menuName = "Space ace/Configs/Auxiliary/Easing service config")]
    public sealed class EasingServiceConfig : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve _entranceFast;

        public AnimationCurve EntranceFast => _entranceFast;

        [SerializeField]
        private AnimationCurve _entranceSlow;

        public AnimationCurve EntranceSlow => _entranceSlow;

        [SerializeField]
        private AnimationCurve _entranceSmooth;

        public AnimationCurve EntranceSmooth => _entranceSmooth;

        [SerializeField]
        private AnimationCurve _entranceSharp;

        public AnimationCurve EntranceSharp => _entranceSharp;

        [SerializeField]
        private AnimationCurve _entranceBouncy;

        public AnimationCurve EntranceBouncy => _entranceBouncy;
    }
}