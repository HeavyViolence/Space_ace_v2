using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories.ProjectileFactories;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class PiercingAmmoSet : AmmoSet, IEquatable<PiercingAmmoSet>
    {
        private float _currentHeatGenerationFactor = 1f;

        public int ProjectileHits { get; }

        public float HeatGenerationFactorPerShot { get; }
        public float HeatGenerationIncreasePerShotPercentage => (HeatGenerationFactorPerShot - 1f) * 100f;

        protected override ShotBehaviourAsync ShotBehaviourAsync => async delegate (object user, IGun gun)
        {
            CachedProjectile projectile = Services.ProjectileFactory.Create(user, ProjectileSkin, Size);

            float dispersion = AuxMath.RandomUnit * gun.Dispersion;

            Vector2 projectileDirection = new(gun.Transform.up.x + gun.SignedConvergenceAngle + dispersion, gun.Transform.up.y);
            projectileDirection.Normalize();

            Quaternion projectileRotation = gun.Transform.rotation * Quaternion.Euler(0f, 0f, gun.SignedConvergenceAngle + dispersion);
            projectileRotation.Normalize();

            MovementData data = new(Speed, Speed, 0f, gun.Transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

            projectile.Object.transform.SetPositionAndRotation(gun.Transform.position, projectileRotation);
            projectile.MovementBehaviourSupplier.Supply(MovementBehaviour, data);

            projectile.DamageDealer.Hit += (sender, hitArgs) =>
            {
                HitBehaviour?.Invoke(hitArgs);
                Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, hitArgs.HitPosition).Forget();

                if (projectile.DamageDealer.HitCount == ProjectileHits)
                    Services.ProjectileFactory.Release(projectile, ProjectileSkin);
            };

            projectile.Escapable.Escaped += (sender, args) =>
            {
                MissBehaviour?.Invoke();
                Services.ProjectileFactory.Release(projectile, ProjectileSkin);
            };

            Amount--;
            Price -= ShotPrice;

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();

            if (user is IAmmoObservable observable)
            {
                if (observable.Shooter.FirstShotInLine == true) _currentHeatGenerationFactor = 1f;
                else _currentHeatGenerationFactor *= HeatGenerationFactorPerShot;
            }
            else
            {
                throw new Exception($"{nameof(PiercingAmmoSet)} user doesn't implement {typeof(IAmmoObservable)}!");
            }

            await UniTask.WaitForSeconds(1f / gun.FireRate);

            return new(1, HeatGeneration * _currentHeatGenerationFactor);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        protected override HitBehaviour HitBehaviour => delegate (HitEventArgs hitArgs)
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