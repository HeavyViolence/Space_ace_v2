using NaughtyAttributes;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Experience
{
    [CreateAssetMenu(fileName = "Experience collector config",
                     menuName = "Space ace/Configs/Experience collector config")]
    public sealed class ExperienceCollectorConfig : ScriptableObject
    {
        #region factor

        public const float MinExperienceFactor = 0.1f;
        public const float MaxExperienceFactor = 10f;
        public const float DefaultExperienceFactor = 1f;

        [SerializeField, Range(MinExperienceFactor, MaxExperienceFactor)]
        private float _experienceFactor = DefaultExperienceFactor;

        public float ExperienceFactor => _experienceFactor;

        #endregion

        #region dissipation over time

        [SerializeField, HorizontalLine]
        private bool _enableDissipation = false;

        private const float MinDissipationDuration = 10f;
        private const float MaxDissipationduration = 300f;

        [SerializeField, Range(MinDissipationDuration, MaxDissipationduration), ShowIf("_enableDissipation")]
        private float _dissipationDuration = MinDissipationDuration;

        [SerializeField, ShowIf("_enableDissipation")]
        private AnimationCurve _dissipationOverTime;

        public float GetExperienceLossFactor(float lifeTime)
        {
            if (lifeTime < 0f) throw new ArgumentOutOfRangeException();
            return _enableDissipation == true ? 1f - _dissipationOverTime.Evaluate(lifeTime / _dissipationDuration) : 1f; 
        }

        #endregion
    }
}