using NaughtyAttributes;

using SpaceAce.Gameplay.Items;
using SpaceAce.Main.Audio;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting
{
    [CreateAssetMenu(fileName = "Shooting config",
                     menuName = "Space ace/Configs/Shooting/Shooting config")]
    public sealed class ShootingConfig : ScriptableObject
    {
        #region heat

        public const float MinHeatCapacity = 100f;
        public const float MaxHeatCapacity = 1_000f;

        public const float MinBaseHeatLossRate = 1f;
        public const float MaxBaseHeatLossRate = 100f;

        public const float MinOverheatDuration = 1f;
        public const float MaxOverheatDuration = 10f;

        [SerializeField, MinMaxSlider(MinHeatCapacity, MaxHeatCapacity)]
        private Vector2 _heatCapacity = new(MinHeatCapacity, MaxHeatCapacity);

        public float MinInitialHeatCapacity => _heatCapacity.x;
        public float MaxInitialHeatCapacity => _heatCapacity.y;
        public float RandomInitialHeatCapacity => Random.Range(_heatCapacity.x, _heatCapacity.y);

        [SerializeField, MinMaxSlider(MinBaseHeatLossRate, MaxBaseHeatLossRate)]
        private Vector2 _baseHeatLossRate = new(MinBaseHeatLossRate, MaxBaseHeatLossRate);

        public float MinInitialBaseHeatLossRate => _baseHeatLossRate.x;
        public float MaxInitialBaseHeatLossRate => _baseHeatLossRate.y;
        public float RandomInitialBaseHeatLossRate => Random.Range(_baseHeatLossRate.x, _baseHeatLossRate.y);

        [SerializeField]
        private AnimationCurve _heatLossFactorCurve;

        public AnimationCurve HeatLossFactorCurve => _heatLossFactorCurve;

        [SerializeField, Space]
        private AudioCollection _overheatAudio;

        public AudioCollection OverheatAudio => _overheatAudio;

        [SerializeField, Range(MinOverheatDuration, MaxOverheatDuration)]
        private float _overheatDuration = MinOverheatDuration;

        public float OverheatDuration => _overheatDuration;

        #endregion

        #region small guns switch

        public const float MinGunsSwitchDuration = 0.1f;
        public const float MaxGunsSwitchDuration = 1f;

        [SerializeField, Space]
        private AudioCollection _smallGunsSwitchAudio;

        [SerializeField, Range(MinGunsSwitchDuration, MaxGunsSwitchDuration)]
        private float _smallGunsSwitchDuration = MinGunsSwitchDuration;

        #endregion

        #region medium guns switch

        [SerializeField, Space]
        private AudioCollection _mediumGunsSwitchAudio;

        [SerializeField, Range(MinGunsSwitchDuration, MaxGunsSwitchDuration)]
        private float _mediumGunsSwitchDuration = MinGunsSwitchDuration;

        #endregion

        #region large guns switch

        [SerializeField, Space]
        private AudioCollection _largeGunsSwitchAudio;

        [SerializeField, Range(MinGunsSwitchDuration, MaxGunsSwitchDuration)]
        private float _largeGunsSwitchDuration = MinGunsSwitchDuration;

        #endregion

        #region guns switch duration

        public AudioCollection GetGunsSwitchAudio(Size size)
        {
            return size switch
            {
                Size.Small => _smallGunsSwitchAudio,
                Size.Medium => _mediumGunsSwitchAudio,
                Size.Large => _largeGunsSwitchAudio,
                _ => _mediumGunsSwitchAudio
            };
        }

        public float GetGunsSwitchDuration(Size size)
        {
            return size switch
            {
                Size.Small => _smallGunsSwitchDuration,
                Size.Medium => _mediumGunsSwitchDuration,
                Size.Large => _largeGunsSwitchDuration,
                _ => _mediumGunsSwitchDuration
            };
        }

        #endregion

        #region first guns switch

        [SerializeField, Space]
        private bool _enableFirstGunsSwitchAudio = false;

        public bool FirstGunsSwitchAudioEnabled => _enableFirstGunsSwitchAudio;

        #endregion

        #region ammo

        public const float MinAmmoSwitchDuration = 0.2f;
        public const float MaxAmmoSwitchDuration = 2f;

        [SerializeField, Space]
        private AudioCollection _emptyAmmoAudio;

        public AudioCollection EmptyAmmoAudio => _emptyAmmoAudio;

        [SerializeField]
        private AudioCollection _ammoSwitchAudio;

        public AudioCollection AmmoSwitchAudio => _ammoSwitchAudio;

        [SerializeField, Range(MinAmmoSwitchDuration, MaxAmmoSwitchDuration)]
        private float _ammoSwitchDuration = MinAmmoSwitchDuration;

        public float AmmoSwitchDuration => _ammoSwitchDuration;

        #endregion
    }
}