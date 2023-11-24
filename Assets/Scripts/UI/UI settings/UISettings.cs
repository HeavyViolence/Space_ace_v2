using SpaceAce.Main;

using UnityEngine;

namespace SpaceAce.UI
{
    [CreateAssetMenu(fileName = "UI settings",
                     menuName = "Space ace/Configs/UI/UI settings")]
    public sealed class UISettings : ScriptableObject
    {
        public const float FadingDurationFactor = 2f;

        [SerializeField]
        private AnimationCurve _fadingCurve;

        [SerializeField, Range(GameStateLoader.MinLoadingDelay, GameStateLoader.MaxLoadingDelay)]
        private float _loadingDelay = GameStateLoader.MinLoadingDelay;

        public AnimationCurve FadingCurve => _fadingCurve;

        public float LoadingDelay => _loadingDelay;
        public float FadingDuration => _loadingDelay * FadingDurationFactor;
    }
}