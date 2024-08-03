using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Experience
{
    public sealed class ExperienceCollector : MonoBehaviour
    {
        [SerializeField]
        private ExperienceCollectorConfig _config;

        private List<IExperienceSource> _experienceSources;

        private void Awake()
        {
            _experienceSources = new(FindAllExperienceSources());
        }

        private IEnumerable<IExperienceSource> FindAllExperienceSources()
        {
            List<IExperienceSource> sources = new();

            foreach (IExperienceSource source in gameObject.GetComponents<IExperienceSource>())
                sources.Add(source);

            return sources;
        }

        public ExperienceDrop GetExperience(float lifeTime)
        {
            float experienceRaw = 0f;
            float experienceLossFactor = _config.GetExperienceLossFactor(lifeTime);

            foreach (IExperienceSource source in _experienceSources) experienceRaw += source.GetExperience();

            float experienceGain = experienceRaw * experienceLossFactor * _config.ExperienceFactor;
            float experienceLoss = experienceRaw * (1f - experienceLossFactor) * _config.ExperienceFactor;

            return new(experienceGain, experienceLoss);
        }
    }
}