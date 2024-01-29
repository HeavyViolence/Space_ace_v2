using SpaceAce.Gameplay.Items;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Guns
{
    public sealed class Gun : MonoBehaviour
    {
        [SerializeField]
        private GunConfig _config;

        public Size AmmoSize => _config.AmmoSize;
        public bool IsRightHanded => transform.localPosition.x > 0f;
        public float SignedConvergenceAngle => IsRightHanded ? -1f * _config.ConvergenceAngle : _config.ConvergenceAngle;
        public float FireRate => _config.FireRate;
        public float Dispersion => _config.Dispersion;
    }
}