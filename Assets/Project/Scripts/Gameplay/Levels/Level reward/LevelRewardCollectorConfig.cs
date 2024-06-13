using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Levels
{
    [CreateAssetMenu(fileName = "Level reward collector config",
                     menuName = "Space ace/Configs/Level reward collector config")]
    public sealed class LevelRewardCollectorConfig : ScriptableObject
    {
        #region level credits reward

        public const float MinFirstLevelCreditsReward = 0f;
        public const float MaxFirstLevelCreditsReward = 1_000f;

        public const float MinCreditsRewardIncreasePerLevel = 0f;
        public const float MaxCreditsRewardIncreasePerLevel = 1_000f;

        [SerializeField, Range(MinFirstLevelCreditsReward, MaxFirstLevelCreditsReward)]
        private float _firstLevelCreditsReward = MinFirstLevelCreditsReward;

        [SerializeField, Range(MinCreditsRewardIncreasePerLevel, MaxCreditsRewardIncreasePerLevel)]
        private float _creditsRewardIncreasePerLevel = MinCreditsRewardIncreasePerLevel;

        public float GetBaseCreditsReward(int level)
        {
            if (level <= 0) throw new ArgumentOutOfRangeException();
            return _firstLevelCreditsReward + _creditsRewardIncreasePerLevel * (level - 1);
        }

        #endregion

        #region level experience reward

        public const float MinFirstLevelExperienceReward = 0f;
        public const float MaxFirstLevelExperienceReward = 10_000f;

        public const float MinExperienceRewardIncreasePerLevel = 0f;
        public const float MaxExperienceRewardIncreasePerLevel = 10_000f;

        [SerializeField, Range(MinFirstLevelExperienceReward, MaxFirstLevelExperienceReward), Space]
        private float _firstLevelExperienceReward = MinFirstLevelExperienceReward;

        [SerializeField, Range(MinExperienceRewardIncreasePerLevel, MaxExperienceRewardIncreasePerLevel)]
        private float _experienceRewardIncreasePerLevel = MinExperienceRewardIncreasePerLevel;

        public float GetBaseExperienceReward(int level)
        {
            if (level <= 0) throw new ArgumentOutOfRangeException();
            return _firstLevelExperienceReward + _experienceRewardIncreasePerLevel * (level - 1);
        }

        #endregion
    }
}