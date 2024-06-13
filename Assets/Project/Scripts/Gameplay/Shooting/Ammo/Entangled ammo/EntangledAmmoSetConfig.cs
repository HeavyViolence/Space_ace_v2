using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Entangled ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Entangled ammo set config")]
    public sealed class EntangledAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Entangled;

        #region ammo loss on miss

        public const int MinAmmoLossOnMiss = 1;
        public const int MaxAmmoLossOnMiss = 20;

        [SerializeField, MinMaxSlider(MinAmmoLossOnMiss, MaxAmmoLossOnMiss), Space]
        private Vector2Int _ammoLossOnMiss = new(MinAmmoLossOnMiss, MaxAmmoLossOnMiss);

        public Vector2Int AmmoLossOnMiss => _ammoLossOnMiss;

        #endregion

        #region ammo gain on hit

        public const int MinAmmoGainOnHit = 1;
        public const int MaxAmmoGainOnHit = 20;

        [SerializeField, MinMaxSlider(MinAmmoGainOnHit, MaxAmmoGainOnHit)]
        private Vector2Int _ammoGainOnHit = new(MinAmmoGainOnHit, MaxAmmoGainOnHit);

        public Vector2Int AmmoGainOnHit => _ammoGainOnHit;

        #endregion
    }
}