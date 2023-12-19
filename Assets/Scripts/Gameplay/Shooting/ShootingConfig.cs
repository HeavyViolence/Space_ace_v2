using NaughtyAttributes;

using SpaceAce.Auxiliary;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting
{
    [CreateAssetMenu(fileName = "Shooting config",
                     menuName = "Space ace/Configs/Shooting/Shooting config")]
    public sealed class ShootingConfig : ScriptableObject
    {
        public const float MinHeatCapacity = 100f;
        public const float MaxHeatCapacity = 1_000f;

        public const float MinBaseHeatLossRate = 1f;
        public const float MaxBaseHeatLossRate = 100f;

        public const float MinOverheatDuration = 0f;
        public const float MaxOverheatDuration = 10f;

        [SerializeField, MinMaxSlider(MinHeatCapacity, MaxHeatCapacity)]
        private Vector2 _heatCapacity = new(MinHeatCapacity, MaxHeatCapacity);

        public float MinInitialHeatCapacity => _heatCapacity.x;
        public float MaxInitialHeatCapacity => _heatCapacity.y;
        public float RandomInitialHeatCapacity => AuxMath.GetRandom(_heatCapacity.x, _heatCapacity.y);

        [SerializeField, MinMaxSlider(MinBaseHeatLossRate, MaxBaseHeatLossRate)]
        private Vector2 _baseHeatLossRate = new(MinBaseHeatLossRate, MaxBaseHeatLossRate);

        public float MinInitialBaseHeatLossRate => _baseHeatLossRate.x;
        public float MaxInitialBaseHeatLossRate => _baseHeatLossRate.y;
        public float RandomInitialBaseHeatLossRate => AuxMath.GetRandom(_baseHeatLossRate.x, _baseHeatLossRate.y);

        [SerializeField, MinMaxSlider(MinOverheatDuration, MaxOverheatDuration)]
        private Vector2 _overheatDuration = new(MinOverheatDuration, MaxOverheatDuration);

        public float MinInitialOverheatDuration => _overheatDuration.x;
        public float MaxInitialOverheatDuration => _overheatDuration.y;
        public float RandomInitialOverheatDuration => AuxMath.GetRandom(_overheatDuration.x, _overheatDuration.y);

        [SerializeField]
        private AnimationCurve _heatLossFactorCurve;

        public AnimationCurve HeatLossFactorCurve => _heatLossFactorCurve;
    }
}