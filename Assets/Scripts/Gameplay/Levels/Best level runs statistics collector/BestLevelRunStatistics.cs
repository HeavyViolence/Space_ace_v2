using Newtonsoft.Json;
using System;
using UnityEngine;

namespace SpaceAce.Gameplay.Levels
{
    public sealed class BestLevelRunStatistics
    {
        public static BestLevelRunStatistics Default => new(0f, 0f, 0f, 0f, 0, 0, 0, 0, 0, 0, 0f, 0f, 0, 0f, TimeSpan.Zero);

        public float DamageAverted { get; }
        public float DamageTaken { get; }

        [JsonIgnore]
        public float DamageAvertedMastery => DamageTaken == 0f ? 1f : DamageAverted / DamageTaken;

        public float DamageSent { get; }
        public float DamageDelivered { get; }

        [JsonIgnore]
        public float DamageDealtMastery => DamageSent == 0f ? 0f : DamageDelivered / DamageSent;

        public int ShotsFired { get; }
        public int Hits { get; }

        [JsonIgnore]
        public float Accuracy => ShotsFired == 0 ? 0f : (float)Hits / ShotsFired;

        public int MeteorsEncountered { get; }
        public int MeteorsDestroyed { get; }

        [JsonIgnore]
        public float MeteorsMastery => MeteorsEncountered == 0 ? 0f : (float)MeteorsDestroyed / MeteorsEncountered;

        public int SpaceDebrisEncountered { get; }
        public int SpaceDebrisDestroyed { get; }

        [JsonIgnore]
        public float SpaceDebrisMastery => SpaceDebrisEncountered == 0 ? 0f : (float)SpaceDebrisDestroyed / SpaceDebrisEncountered;

        public float ExperienceEarned { get; }
        public float ExperienceLost { get; }

        [JsonIgnore]
        public float ExperienceMatery => ExperienceEarned + ExperienceLost == 0f ? 0f : ExperienceEarned / (ExperienceEarned + ExperienceLost);

        [JsonIgnore]
        public float LevelMastery => DamageAvertedMastery *
                                     DamageDealtMastery *
                                     Accuracy *
                                     MeteorsMastery *
                                     SpaceDebrisMastery *
                                     ExperienceMatery;

        public int EnemiesDefeated { get; }
        public float CreditsEarned { get; }
        public TimeSpan RunTime { get; }

        public BestLevelRunStatistics(float damageAverted,
                                      float damageTaken,
                                      float damageSent,
                                      float damageDelivered,
                                      int shotsFired,
                                      int hits,
                                      int meteorsEncountered,
                                      int meteorsDestroyed,
                                      int spaceDebrisEncountered,
                                      int spaceDebrisDestroyed,
                                      float experienceEarned,
                                      float experienceLost,
                                      int enemiesDefeated,
                                      float creditsEarned,
                                      TimeSpan runTime)
        {
            DamageAverted = Mathf.Clamp(damageAverted, 0f, float.MaxValue);
            DamageTaken = Mathf.Clamp(damageTaken, 0f, float.MaxValue);
            DamageSent = Mathf.Clamp(damageSent, 0f, float.MaxValue);
            DamageDelivered = Mathf.Clamp(damageDelivered, 0f, float.MaxValue);
            ShotsFired = Mathf.Clamp(shotsFired, 0, int.MaxValue);
            Hits = Mathf.Clamp(hits, 0, int.MaxValue);
            MeteorsEncountered = Mathf.Clamp(meteorsEncountered, 0, int.MaxValue);
            MeteorsDestroyed = Mathf.Clamp(meteorsDestroyed, 0, int.MaxValue);
            SpaceDebrisEncountered = Mathf.Clamp(spaceDebrisEncountered, 0, int.MaxValue);
            SpaceDebrisDestroyed = Mathf.Clamp(spaceDebrisDestroyed, 0, int.MaxValue);
            ExperienceEarned = Mathf.Clamp(experienceEarned, 0f, float.MaxValue);
            ExperienceLost = Mathf.Clamp(experienceLost, 0f, float.MaxValue);
            EnemiesDefeated = Mathf.Clamp(enemiesDefeated, 0, int.MaxValue);
            CreditsEarned = Mathf.Clamp(creditsEarned, 0f, float.MaxValue);
            RunTime = runTime;
        }
    }
}