using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Strange ammo config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Strange ammo config")]
    public sealed class StrangeAmmoSetConfig : AmmoSetConfig
    {
        public const float MinAmmoLossProbability = 0f;
        public const float MaxAmmoLossProbability = 1f;

        [SerializeField, MinMaxSlider(MinAmmoLossProbability, MaxAmmoLossProbability), Space]
        private Vector2 _ammoLossProbability = new(MinAmmoLossProbability, MaxAmmoLossProbability);

        public Vector2 AmmoLossProbability => _ammoLossProbability;

        public override AmmoType AmmoType => AmmoType.Strange;
    }
}