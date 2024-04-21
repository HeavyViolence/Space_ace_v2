using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class CatalyticAmmoSet : AmmoSet, IEquatable<CatalyticAmmoSet>
    {
        private float _currentFireRateFactorPerShot = 1f;

        public float FireRateFactorPerShot { get; }
        public float FireRateIncreasePerShotPercentage => (FireRateFactorPerShot - 1f) * 100f;

        protected override ShotBehaviourAsync ShotBehaviourAsync => async delegate (object user, Gun gun, object[] args)
        {
            CachedProjectile projectile = Services.ProjectileFactory.Create(user, ProjectileSkin, Size);

            float dispersion = AuxMath.RandomUnit * gun.Dispersion;

            Vector2 projectileDirection = new(gun.transform.up.x + gun.SignedConvergenceAngle + dispersion, gun.transform.up.y);
            projectileDirection.Normalize();

            Quaternion projectileRotation = gun.transform.rotation * Quaternion.Euler(0f, 0f, gun.SignedConvergenceAngle + dispersion);
            projectileRotation.Normalize();

            MovementData data = new(Speed, Speed, 0f, gun.transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

            projectile.Instance.transform.SetPositionAndRotation(gun.transform.position, projectileRotation);
            projectile.MovementBehaviourSupplier.Supply(MovementBehaviour, data);

            projectile.DamageDealer.Hit += (sender, hitArgs) =>
            {
                HitBehaviour?.Invoke(hitArgs, args);
                Services.ProjectileFactory.Release(projectile, ProjectileSkin);
                Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, hitArgs.HitPosition).Forget();
            };

            projectile.Escapable.WaitForEscapeAsync().Forget();

            projectile.Escapable.Escaped += (sender, args) =>
            {
                MissBehaviour?.Invoke(args);
                Services.ProjectileFactory.Release(projectile, ProjectileSkin);
            };

            Amount--;
            Price -= ShotPrice;

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.transform.position, null, true).Forget();

            if (user is IAmmoObservable observable)
            {
                if (observable.Shooter.FirstShotInLine == true) _currentFireRateFactorPerShot = 1f;
                else _currentFireRateFactorPerShot *= FireRateFactorPerShot;
            }
            else
            {
                throw new Exception($"{nameof(CatalyticAmmoSet)} user doesn't implement {typeof(IAmmoObservable)}!");
            }

            float clampedFireRate = Mathf.Clamp(gun.FireRate * _currentFireRateFactorPerShot, GunConfig.MinFireRate, GunConfig.MaxFireRate);
            await UniTask.WaitForSeconds(1f / clampedFireRate);

            return new(1, HeatGeneration);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            Vector2 velocity = data.InitialVelocity * Time.fixedDeltaTime;
            body.MovePosition(body.position + velocity);
        };

        protected override HitBehaviour HitBehaviour => delegate (HitEventArgs hitArgs, object[] args)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);
        };

        protected override MissBehaviour MissBehaviour => null;

        public CatalyticAmmoSet(AmmoServices services,
                                CatalyticAmmoSetConfig config,
                                CatalyticAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            FireRateFactorPerShot = savedState.FirerateFactorPerShot;
        }

        public CatalyticAmmoSet(AmmoServices services,
                                Size size,
                                Quality quality,
                                CatalyticAmmoSetConfig config) : base(services, size, quality, config)
        {
            FireRateFactorPerShot = services.ItemPropertyEvaluator.Evaluate(config.FireRateFactorPerShot, true, quality, size, SizeInfluence.None);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Catalytic/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Catalytic/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Catalytic/Code", this);

        public override ItemSavableState GetSavableState() =>
            new CatalyticAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, FireRateFactorPerShot);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as CatalyticAmmoSet);

        public bool Equals(CatalyticAmmoSet other) => other is not null &&
                                                      FireRateFactorPerShot == other.FireRateFactorPerShot;

        public override int GetHashCode() => base.GetHashCode() ^ FireRateFactorPerShot.GetHashCode();

        #endregion
    }
}