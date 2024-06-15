using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories.ProjectileFactories;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class AntimatterAmmoSet : AmmoSet
    {
        public float DamageFactorPerShot { get; }
        public float DamageIncreasePerShotPercentage => DamageFactorPerShot * 100f;

        public float FireRateFactorPerShot { get; }
        public float FireRateDecreasePerShotPercentage => FireRateFactorPerShot * 100f;

        public AntimatterAmmoSet(AmmoServices services,
                                 AntimatterAmmoSetConfig config,
                                 AntimatterAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            DamageFactorPerShot = savedState.DamageFactorPerShot;
            FireRateFactorPerShot = savedState.FireRateFactorPerShot;
        }

        public AntimatterAmmoSet(AmmoServices services,
                                 Size size,
                                 Quality quality,
                                 AntimatterAmmoSetConfig config) : base(services, size, quality, config)
        {
            DamageFactorPerShot = services.ItemPropertyEvaluator.Evaluate(config.DamageFactorPerShot,
                                                                          RangeEvaluationDirection.Forward,
                                                                          quality,
                                                                          size,
                                                                          SizeInfluence.None);

            FireRateFactorPerShot = services.ItemPropertyEvaluator.Evaluate(config.FireRateFactorPerShot,
                                                                            RangeEvaluationDirection.Forward,
                                                                            quality,
                                                                            size,
                                                                            SizeInfluence.None);
        }

        public override async UniTask FireAsync(object shooter,
                                                IGun gun,
                                                CancellationToken fireCancellation = default,
                                                CancellationToken overheatCancellation = default)
        {
            if (shooter is null) throw new ArgumentNullException();
            if (gun is null) throw new ArgumentNullException();

            float damageFactor = 1f;
            float fireRateFactor = 1f;

            while (Amount > 0 && fireCancellation.IsCancellationRequested == false && overheatCancellation.IsCancellationRequested == false)
            {
                if (AuxMath.RandomNormal < EMPFactor)
                {
                    await UniTask.WaitUntil(() => Services.GamePauser.Paused == false);
                    await UniTask.WaitForSeconds(1f / (gun.FireRate * fireRateFactor));

                    continue;
                }

                ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

                float dispersion = AuxMath.RandomUnit * gun.Dispersion;

                Vector2 projectileDirection = new(gun.Transform.up.x + gun.SignedConvergenceAngle + dispersion, gun.Transform.up.y);
                projectileDirection.Normalize();

                Quaternion projectileRotation = gun.Transform.rotation * Quaternion.Euler(0f, 0f, gun.SignedConvergenceAngle + dispersion);
                projectileRotation.Normalize();

                MovementData data = new(Speed, Speed, 0f, gun.Transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

                projectile.Transform.SetPositionAndRotation(gun.Transform.position, projectileRotation);
                projectile.MovementBehaviourSupplier.Supply(OnMove, data);

                projectile.DamageDealer.Hit += (_, e) =>
                {
                    OnHit(shooter, e, damageFactor);
                    Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                    Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, e.HitPosition).Forget();
                };

                projectile.Escapable.Escaped += (_, _) => Services.ProjectileFactory.Release(ProjectileSkin, projectile);

                if (shooter is Player) Amount--;

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();
                if (gun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFired();

                OnShotFired(HeatGeneration);

                await UniTask.WaitUntil(() => Services.GamePauser.Paused == false);
                await UniTask.WaitForSeconds(1f / (gun.FireRate * fireRateFactor));

                damageFactor *= DamageFactorPerShot;
                fireRateFactor *= FireRateFactorPerShot;
            }

            ClearOnShotFired();
        }

        protected override void OnMove(Rigidbody2D body, MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        }

        protected override void OnHit(object shooter, HitEventArgs e, float damageFactor = 0f)
        {
            e.Damageable.ApplyDamage(Damage * damageFactor);
        }

        protected override void OnMiss(object shooter) { }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Antimatter/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Antimatter/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Antimatter/Code", this);

        public override ItemSavableState GetSavableState() =>
            new AntimatterAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, DamageFactorPerShot, FireRateFactorPerShot);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as AntimatterAmmoSet);

        public bool Equals(AntimatterAmmoSet other) => other is not null &&
                                                       DamageFactorPerShot == other.DamageFactorPerShot &&
                                                       FireRateFactorPerShot == other.FireRateFactorPerShot;

        public override int GetHashCode() => base.GetHashCode() ^
                                             DamageFactorPerShot.GetHashCode() ^
                                             FireRateFactorPerShot.GetHashCode();

        #endregion
    }
}