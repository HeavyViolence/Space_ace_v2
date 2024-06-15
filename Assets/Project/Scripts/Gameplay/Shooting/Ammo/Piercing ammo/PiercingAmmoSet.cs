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
    public sealed class PiercingAmmoSet : AmmoSet, IEquatable<PiercingAmmoSet>
    {
        public int ProjectileHits { get; }

        public float HeatGenerationFactorPerShot { get; }
        public float HeatGenerationIncreasePerShotPercentage => (HeatGenerationFactorPerShot - 1f) * 100f;

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
            ProjectileHits = services.ItemPropertyEvaluator.Evaluate(config.ProjectileHits,
                                                                     RangeEvaluationDirection.Forward,
                                                                     quality,
                                                                     size,
                                                                     SizeInfluence.None);

            HeatGenerationFactorPerShot = services.ItemPropertyEvaluator.Evaluate(config.HeatGenerationFactorPerShot,
                                                                                  RangeEvaluationDirection.Backward,
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

            float heatGenerationFactor = 1f;

            while (Amount > 0 && fireCancellation.IsCancellationRequested == false && overheatCancellation.IsCancellationRequested == false)
            {
                if (AuxMath.RandomNormal < EMPFactor)
                {
                    await UniTask.WaitUntil(() => Services.GamePauser.Paused == false);
                    await UniTask.WaitForSeconds(1f / gun.FireRate);

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
                    OnHit(shooter, e);
                    Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, e.HitPosition).Forget();

                    if (projectile.DamageDealer.HitCount == ProjectileHits)
                        Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                };

                projectile.Escapable.Escaped += (_, _) => Services.ProjectileFactory.Release(ProjectileSkin, projectile);

                if (shooter is Player) Amount--;

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();
                if (gun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFired();

                OnShotFired(HeatGeneration * heatGenerationFactor);

                await UniTask.WaitUntil(() => Services.GamePauser.Paused == false);
                await UniTask.WaitForSeconds(1f / gun.FireRate);

                heatGenerationFactor *= HeatGenerationFactorPerShot;
            }

            ClearOnShotFired();
        }

        protected override void OnMove(Rigidbody2D body, MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        }

        protected override void OnHit(object shooter, HitEventArgs e, float damageFactor = 1f)
        {
            e.Damageable.ApplyDamage(Damage);
        }

        protected override void OnMiss(object shooter) { }

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