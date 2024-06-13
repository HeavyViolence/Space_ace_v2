using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Critical ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Critical ammo set config")]
    public sealed class CriticalAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Critical;

        #region critical damage probability

        public const float MinCriticalDamageProbability = 0f;
        public const float MaxCriticalDamageProbability = 1f;

        [SerializeField, MinMaxSlider(MinCriticalDamageProbability, MaxCriticalDamageProbability), Space]
        private Vector2 _criticalDamageProbability = new(MinCriticalDamageProbability, MaxCriticalDamageProbability);

        public Vector2 CriticalDamageProbability => _criticalDamageProbability;

        #endregion

        #region critical damage

        public const float MinCriticalDamage = 50f;
        public const float MaxCriticalDamage = 1000f;

        [SerializeField, MinMaxSlider(MinCriticalDamage, MaxCriticalDamage)]
        private Vector2 _criticalDamage = new(MinCriticalDamage, MaxCriticalDamage);

        public Vector2 CriticalDamage => _criticalDamage;

        #endregion
    }
}