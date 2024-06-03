using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories.ProjectileFactories;

using System;

using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class PiercingAmmoSet : AmmoSet, IEquatable<PiercingAmmoSet>
    {
        private float _currentHeatGenerationFactor = 1f;

        public int ProjectileHits { get; }

        public float HeatGenerationFactorPerShot { get; }
        public float HeatGenerationIncreasePerShotPercentage => (HeatGenerationFactorPerShot - 1f) * 100f;

        public override ShotBehaviour ShotBehaviour => delegate (object shooter, IGunView gunView)
        {
            ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

            float dispersion = AuxMath.RandomUnit * gunView.Dispersion;

            Vector2 projectileDirection = new(gunView.Transform.up.x + gunView.SignedConvergenceAngle + dispersion, gunView.Transform.up.y);
            projectileDirection.Normalize();

            Quaternion projectileRotation = gunView.Transform.rotation * Quaternion.Euler(0f, 0f, gunView.SignedConvergenceAngle + dispersion);
            projectileRotation.Normalize();

            MovementData data = new(Speed, Speed, 0f, gunView.Transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

            projectile.Transform.SetPositionAndRotation(gunView.Transform.position, projectileRotation);
            projectile.MovementBehaviourSupplier.Supply(MovementBehaviour, data);

            projectile.DamageDealer.Hit += (sender, hitArgs) =>
            {
                HitBehaviour?.Invoke(shooter, hitArgs);
                Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, hitArgs.HitPosition).Forget();

                if (projectile.DamageDealer.HitCount == ProjectileHits)
                    Services.ProjectileFactory.Release(ProjectileSkin, projectile);
            };

            projectile.Escapable.Escaped += (sender, args) =>
            {
                MissBehaviour?.Invoke(shooter);
                Services.ProjectileFactory.Release(ProjectileSkin, projectile);
            };

            if (shooter is Player)
            {
                Amount--;
                Price -= ShotPrice;
            }

            if (gunView.FirstShotInLine == true) _currentHeatGenerationFactor = 1f;
            else _currentHeatGenerationFactor *= HeatGenerationFactorPerShot;

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gunView.Transform.position, null, true).Forget();

            return new(1, HeatGeneration * _currentHeatGenerationFactor);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        protected override HitBehaviour HitBehaviour => delegate (object shooter, HitEventArgs hitArgs)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);
        };

        protected override MissBehaviour MissBehaviour => null;

        public PiercingAmmoSet(AmmoServices services,
                               PiercingAmmoSetConfig config,
                               PiercingAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            ProjectileHits = savedState.ProjectileHits;
            HeatGenerationFactorPerShot = savedState.HeatGenerationFactorPerShot;
        }

        public PiercingAmmoSet(AmmoServices services,
                               Size size,
                               Quality quality,
                               PiercingAmmoSetConfig config) : base(services, size, quality, config)
        {
            ProjectileHits = services.ItemPropertyEvaluator.Evaluate(config.ProjectileHits, true, quality, size, SizeInfluence.None);
            HeatGenerationFactorPerShot = services.ItemPropertyEvaluator.Evaluate(config.HeatGenerationFactorPerShot, false, quality, size, SizeInfluence.None);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Piercing/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Piercing/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Piercing/Code", this);

        public override ItemSavableState GetSavableState() =>
            new PiercingAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, ProjectileHits, HeatGenerationFactorPerShot);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as PiercingAmmoSet);

        public bool Equals(PiercingAmmoSet other) => other is not null &&
                                                     ProjectileHits == other.ProjectileHits &&
                                                     HeatGenerationFactorPerShot == other.HeatGenerationFactorPerShot;

        public override int GetHashCode() => base.GetHashCode() ^
                                             ProjectileHits.GetHashCode() ^
                                             HeatGenerationFactorPerShot.GetHashCode();

        #endregion
    }
}