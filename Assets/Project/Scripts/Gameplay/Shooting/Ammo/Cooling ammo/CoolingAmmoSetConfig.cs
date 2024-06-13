using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Cooling ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Cooling ammo set config")]
    public sealed class CoolingAmmoSetConfig : AmmoSetConfig
    {
        public const float MinHeatGenerationFactorPerShot = 0.1f;
        public const float MaxHeatGenerationFactorPerShot = 1f;

        [SerializeField, MinMaxSlider(MinHeatGenerationFactorPerShot, MaxHeatGenerationFactorPerShot), Space]
        private Vector2 _heatGenerationFactorPerShot = new(MinHeatGenerationFactorPerShot, MaxHeatGenerationFactorPerShot);

        public Vector2 HeatGenerationFactorPerShot => _heatGenerationFactorPerShot;

        public override AmmoType AmmoType => AmmoType.Cooling;
    }
}