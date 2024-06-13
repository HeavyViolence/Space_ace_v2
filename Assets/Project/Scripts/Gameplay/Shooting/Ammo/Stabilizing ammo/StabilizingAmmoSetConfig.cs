using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Stabilizing ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Stabilizing ammo set config")]
    public sealed class StabilizingAmmoSetConfig : AmmoSetConfig
    {
        public const float MinDispersionFactorPerShot = 0f;
        public const float MaxDoispersionFactorPerShot = 1f;

        [SerializeField, MinMaxSlider(MinDispersionFactorPerShot, MaxDoispersionFactorPerShot), Space]
        private Vector2 _dispersionFactorPerShot = new(MinDispersionFactorPerShot, MaxDoispersionFactorPerShot);

        public Vector2 DispersionFactorPerShot => _dispersionFactorPerShot;

        public override AmmoType AmmoType => AmmoType.Stabilizing;
    }
}