using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Devastating ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Devastating ammo set config")]
    public sealed class DevastatingAmmoSetConfig : AmmoSetConfig
    {
        public const float MinConsecutiveDamageFactor = 1f;
        public const float MaxConsecutiveDamageFactor = 2f;

        [SerializeField, MinMaxSlider(MinConsecutiveDamageFactor, MaxConsecutiveDamageFactor), Space]
        private Vector2 _consecutiveDamageFactor = new(MinConsecutiveDamageFactor, MaxConsecutiveDamageFactor);

        public Vector2 ConsecutiveDamegeFactor => _consecutiveDamageFactor;

        public override AmmoType AmmoType => AmmoType.Devastating;
    }
}