using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Gameplay.Items;
using SpaceAce.Main.Factories.EnemyShipFactories;

using UnityEngine;

using System;
using System.Collections.Generic;

using NaughtyAttributes;

namespace SpaceAce.Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "Enemy config",
                     menuName = "Space ace/Configs/Enemies/Enemy config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        #region ship type

        [SerializeField]
        private EnemyShipType _shipType;

        public EnemyShipType ShipType => _shipType;

        #endregion

        #region ammo

        [SerializeField, Space]
        private List<AmmoType> _ammo;

        [SerializeField]
        private Size _ammoSize = Size.Medium;

        public IEnumerable<(AmmoType type, Size size)> GetAssignedAmmo()
        {
            if (_ammo is null || _ammo.Count == 0) throw new Exception("No ammo types assigned!");

            List<(AmmoType type, Size size)> ammo = new(_ammo.Count);

            foreach (AmmoType type in _ammo)
                ammo.Add((type, _ammoSize));

            return ammo;
        }

        #endregion

        #region ammo switching

        public const float MinAmmoSwitchDelay = 0f;
        public const float MaxAmmoSwitchDelay = 60f;

        public bool AmmoSwitchEnabled => _ammo.Count > 1;

        [SerializeField, MinMaxSlider(MinAmmoSwitchDelay, MaxAmmoSwitchDelay), EnableIf("AmmoSwitchEnabled")]
        private Vector2 _ammoSwitchDelay = new(MinAmmoSwitchDelay, MaxAmmoSwitchDelay);

        public float NextAmmoSwitchDelay => AmmoSwitchEnabled == true ? UnityEngine.Random.Range(_ammoSwitchDelay.x, _ammoSwitchDelay.y)
                                                                      : float.PositiveInfinity;

        #endregion

        #region shooting

        public const float MinShootingDuration = 0f;
        public const float MaxShootingDuration = 30f;

        public const float MinShootingPause = 0f;
        public const float MaxShootingPause = 30f;

        [SerializeField, MinMaxSlider(MinShootingDuration, MaxShootingDuration), Space]
        private Vector2 _shootingDuration = new(MinShootingDuration, MaxShootingDuration);

        public float NextShootingDuration => UnityEngine.Random.Range(_shootingDuration.x, _shootingDuration.y);

        [SerializeField, MinMaxSlider(MinShootingPause, MaxShootingPause)]
        private Vector2 _shootingPause = new(MinShootingPause, MaxShootingPause);

        public float NextShootingPause => UnityEngine.Random.Range(_shootingPause.x, _shootingPause.y);

        #endregion

        #region spawn width

        [SerializeField, Space]
        private EnemySpawnWidthConfig _spawnWidthConfig;

        public float GetSpawnWidth(float min, float max) =>
            _spawnWidthConfig.GetSpawnWidth(min, max);

        #endregion

        #region mutations

        [SerializeField]
        private EnemyMutationConfig _mutationConfig;

        public EnemyMutation GetProbableMutation(int level) =>
            _mutationConfig.GetProbableMutation(level);

        #endregion

        #region first level to encounter

        public const int MinLevelToEncounter = 1;
        public const int MaxLevelToEncounter = 10;

        [SerializeField, Range(MinLevelToEncounter, MaxLevelToEncounter)]
        private int _firstLevelToEncounter = MinLevelToEncounter;

        public int FirstLevelToEncounter => _firstLevelToEncounter;

        #endregion
    }
}