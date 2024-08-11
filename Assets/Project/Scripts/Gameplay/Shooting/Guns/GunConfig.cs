using SpaceAce.Gameplay.Items;

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
        private Size _size;

        [SerializeField, Range(MinConvergenceAngle, MaxConvergenceAngle)]
        private float _convergenceAngle = MinConvergenceAngle;

        [SerializeField, Range(MinFireRate, MaxFireRate)]
        private float _fireRate = MinFireRate;

        [SerializeField, Range(MinDispersion, MaxDispersion)]
        private float _dispersion = MinDispersion;

        [SerializeField]
        private bool _shakeOnShotFired = false;

        public Size Size => _size;

        public float ConvergenceAngle => _convergenceAngle * Mathf.Deg2Rad;

        public float FireRate => _fireRate;

        public float Dispersion => _dispersion * Mathf.Deg2Rad;

        public bool ShakeOnShotFired => _shakeOnShotFired;
    }
}