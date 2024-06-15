using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Fusion ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Fusion ammo set config")]
    public sealed class FusionAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Fusion;

        #region armor ignoring

        public const float MinArmorIgnoring = 0f;
        public const float MaxArmorIgnoring = 1f;

        [SerializeField, MinMaxSlider(MinArmorIgnoring, MaxArmorIgnoring), Space]
        private Vector2 _armorIgnoring = new(MinArmorIgnoring, MaxArmorIgnoring);

        public Vector2 ArmorIgnoring => _armorIgnoring;

        #endregion
    }
}