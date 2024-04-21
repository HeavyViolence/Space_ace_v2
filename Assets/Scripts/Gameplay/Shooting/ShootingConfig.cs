using NaughtyAttributes;

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

        [SerializeField]
        private AudioCollection _overheatAudio;

        public AudioCollection OverheatAudio => _overheatAudio;

        [SerializeField, MinMaxSlider(MinOverheatDuration, MaxOverheatDuration)]
        private Vector2 _overheatDuration = new(MinOverheatDuration, MaxOverheatDuration);

        public float MinInitialOverheatDuration => _overheatDuration.x;
        public float MaxInitialOverheatDuration => _overheatDuration.y;
        public float RandomInitialOverheatDuration => Random.Range(MinInitialOverheatDuration, MaxInitialOverheatDuration);

        #endregion

        #region small weapons switch

        public const float MinWeaponsSwitchDuration = 0.2f;
        public const float MaxWeaponsSwitchDuration = 1f;

        [SerializeField, Space]
        private AudioCollection _smallWeaponsSwitchAudio;

        public AudioCollection SmallWeaponsSwitchAudio => _smallWeaponsSwitchAudio;

        [SerializeField, MinMaxSlider(MinWeaponsSwitchDuration, MaxWeaponsSwitchDuration)]
        private Vector2 _smallWeaponsSwitchDuration = new(MinWeaponsSwitchDuration, MaxWeaponsSwitchDuration);

        public float MinSmallWeaponsSwitchDuration => _smallWeaponsSwitchDuration.x;
        public float MaxSmallWeaponsSwitchDuration => _smallWeaponsSwitchDuration.y;
        public float RandomSmallWeaponsSwitchDuration => Random.Range(MinSmallWeaponsSwitchDuration, MaxSmallWeaponsSwitchDuration);

        #endregion

        #region medium weapons switch

        [SerializeField, Space]
        private AudioCollection _mediumWeponsSwitchAudio;

        public AudioCollection MediumWeaponsSwitchAudio => _mediumWeponsSwitchAudio;

        [SerializeField, MinMaxSlider(MinWeaponsSwitchDuration, MaxWeaponsSwitchDuration)]
        private Vector2 _mediumWeaponsSwitchDuration = new(MinWeaponsSwitchDuration, MaxWeaponsSwitchDuration);

        public float MinMediumWeaponsSwitchDuration => _mediumWeaponsSwitchDuration.x;
        public float MaxMediumWeaponsSwitchDuration => _mediumWeaponsSwitchDuration.y;
        public float RandomMediumWeaponsSwitchDuration => Random.Range(MinMediumWeaponsSwitchDuration, MaxMediumWeaponsSwitchDuration);

        #endregion

        #region large weapons switch

        [SerializeField, Space]
        private AudioCollection _largeWeaponsSwitchAudio;

        public AudioCollection LargeWeaponsSwitchAudio => _largeWeaponsSwitchAudio;

        [SerializeField, MinMaxSlider(MinWeaponsSwitchDuration, MaxWeaponsSwitchDuration)]
        private Vector2 _largeWeaponsSwitchDuration = new(MinWeaponsSwitchDuration, MaxWeaponsSwitchDuration);

        public float MinLargeWeaponsSwitchDuration => _largeWeaponsSwitchDuration.x;
        public float MaxLargeWeaponsSwitchDuration => _largeWeaponsSwitchDuration.y;
        public float RandomLargeWeaponsSwitchDuration => Random.Range(MinLargeWeaponsSwitchDuration, MaxLargeWeaponsSwitchDuration);

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

        [SerializeField, MinMaxSlider(MinAmmoSwitchDuration, MaxAmmoSwitchDuration)]
        private Vector2 _ammoSwitchDuration = new(MinAmmoSwitchDuration, MaxAmmoSwitchDuration);

        public float MinInitialAmmoSwitchDuration => _ammoSwitchDuration.x;
        public float MaxInitialAmmoSwitchDuration => _ammoSwitchDuration.y;
        public float RandomInitialAmmoSwitchDuration => Random.Range(MinInitialAmmoSwitchDuration, MaxInitialAmmoSwitchDuration);

        [SerializeField]
        private bool _shakeOnShotFired = false;

        public bool ShakeOnShotFired => _shakeOnShotFired;

        #endregion
    }
}