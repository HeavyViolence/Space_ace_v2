using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main.Factories;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Guns
{
    [CreateAssetMenu(fileName = "Gun config",
                     menuName = "Space ace/Configs/Shooting/Gun config")]
    public sealed class GunConfig : ScriptableObject
    {
        public const float MinConvergenceAngle = 0f;
        public const float MaxConvergenceAngle = 10f;

        public const float MinFireRate = 0.1f;
        public const float MaxFireRate = 30f;

        public const float MinDispersion = 0f;
        public const float MaxDispersion = 10f;

        [SerializeField]
        private ItemSize _ammoSize;

        [SerializeField]
        private ProjectileRequestor _projectileRequestor;

        [SerializeField, Range(MinConvergenceAngle, MaxConvergenceAngle)]
        private float _convergenceAngle = MinConvergenceAngle;

        [SerializeField, Range(MinFireRate, MaxFireRate)]
        private float _fireRate = MinFireRate;

        [SerializeField, Range(MinDispersion, MaxDispersion)]
        private float _dispersion = MinDispersion;

        public ItemSize AmmoSize => _ammoSize;

        public ProjectileRequestor ProjectileRequestor => _projectileRequestor;

        public float ConvergenceAngle => _convergenceAngle;

        public float FireRate => _fireRate;

        public float Dispersion => _dispersion;
    }
}