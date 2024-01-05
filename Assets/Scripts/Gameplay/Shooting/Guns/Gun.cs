using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main.Factories;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Guns
{
    public abstract class Gun : MonoBehaviour
    {
        [SerializeField]
        private GunConfig _config;

        public ItemSize AmmoSize => _config.AmmoSize;
        public ProjectileRequestor ProjectileRequestor => _config.ProjectileRequestor;
        public bool IsRightHanded => transform.localPosition.x > 0f;
        public float SignedConvergenceAngle => IsRightHanded ? -1f * _config.ConvergenceAngle : _config.ConvergenceAngle;

        public virtual float FireRate => _config.FireRate;
        public virtual float Dispersion => _config.Dispersion * AuxMath.RandomUnit;
    }
}