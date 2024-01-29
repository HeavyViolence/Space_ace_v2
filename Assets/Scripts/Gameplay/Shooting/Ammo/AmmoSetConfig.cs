using NaughtyAttributes;

using SpaceAce.Gameplay.Items;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public abstract class AmmoSetConfig : ItemConfig
    {
        [SerializeField, Space]
        private AudioCollection _shotAudio;

        public AudioCollection ShotAudio => _shotAudio;

        [SerializeField]
        private ProjectileSkin _projectileSkin;

        public ProjectileSkin ProjectileSkin => _projectileSkin;

        [SerializeField]
        private ProjectileHitEffectSkin _hitEffectSkin;

        public ProjectileHitEffectSkin HitEffectSkin => _hitEffectSkin;

        #region amount per set

        public const int MinAmount = 100;
        public const int MaxAmount = 9_999;

        [SerializeField, MinMaxSlider(MinAmount, MaxAmount), Space]
        private Vector2Int _amount = new(MinAmount, MaxAmount);

        public Vector2Int Amount => _amount;

        #endregion

        #region heat generation

        public const float MinHeatGeneration = 1f;
        public const float MaxHeatGeneration = 100f;

        [SerializeField, MinMaxSlider(MinHeatGeneration, MaxHeatGeneration)]
        private Vector2 _heatGeneration = new(MinHeatGeneration, MaxHeatGeneration);

        public Vector2 HeatGeneration => _heatGeneration;

        #endregion

        #region speed

        public const float MinSpeed = 70f;
        public const float MaxSpeed = 300f;

        [SerializeField, MinMaxSlider(MinSpeed, MaxSpeed)]
        private Vector2 _speed = new(MinSpeed, MaxSpeed);

        public Vector2 Speed => _speed;

        #endregion

        #region damage

        public const float MinDamage = 10f;
        public const float MaxDamage = 1000f;

        [SerializeField, MinMaxSlider(MinDamage, MaxDamage)]
        private Vector2 _damage = new(MinDamage, MaxDamage);

        public Vector2 Damage => _damage;

        #endregion
    }
}