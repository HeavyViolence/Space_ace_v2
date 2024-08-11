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
    public sealed class StabilizingAmmoSet : AmmoSet, IEquatable<StabilizingAmmoSet>
    {
        public float DispersionFactorPerShot { get; }
        public float DispersionDecreasePerShotPercentage => (1f - DispersionFactorPerShot) * 100f;

        public StabilizingAmmoSet(AmmoServices services,
                                  StabilizingAmmoSetConfig config,
                                  StabilizingAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            DispersionFactorPerShot = savedState.DispersionFactorPerShot;
        }

        public StabilizingAmmoSet(AmmoServices services,
                                  Size size,
                                  Quality quality,
                                  StabilizingAmmoSetConfig config) : base(services, size, quality, config)
        {
            DispersionFactorPerShot = services.ItemPropertyEvaluator.Evaluate(config.DispersionFactorPerShot,
                                                                              RangeEvaluationDirection.Backward,
                                                                              quality,
                                                                              size,
                                                                              SizeInfluence.None);
        }

        public override async UniTask FireAsync(object shooter, IGun gun, CancellationToken token)
        {
            if (shooter is null) throw new ArgumentNullException();
            if (gun is null) throw new ArgumentNullException();

            float dispersionFactor = 1f;

            while (Amount > 0 && token.IsCancellationRequested == false)
            {
                if (AuxMath.RandomNormal < EMPFactor)
                {
                    await AuxAsync.DelayAsync(() => 1f / gun.FireRate, () => Services.GamePauser.Paused == true, token);
                    continue;
                }

                ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

                float dispersion = AuxMath.RandomUnit * gun.Dispersion * dispersionFactor;

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
                    Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                    Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, e.HitPosition).Forget();
                };

                projectile.Escapable.Escaped += (_, _) => Services.ProjectileFactory.Release(ProjectileSkin, projectile);

                if (shooter is Player) Amount--;

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();
                if (gun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFired();

                OnShotFired(HeatGeneration);

                dispersionFactor *= DispersionFactorPerShot;

                await AuxAsync.DelayAsync(() => 1f / gun.FireRate, () => Services.GamePauser.Paused == true, token);
            }

            ClearOnShotFired();
        }

        protected override void OnMove(Rigidbody2D body, MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        }

        protected override void OnHit(object shooter, HitEventArgs hitArgs, float damageFactor = 1f)
        {
            hitArgs.Damageable.ApplyDamage(Damage);
        }

        protected override void OnMiss(object shooter) { }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Stabilizing/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Stabilizing/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Stabilizing/Code", this);

        public override ItemSavableState GetSavableState() =>
            new StabilizingAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, DispersionFactorPerShot);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as StabilizingAmmoSet);

        public bool Equals(StabilizingAmmoSet other) => other is not null &&
                                                        DispersionFactorPerShot == other.DispersionFactorPerShot;

        public override int GetHashCode() => base.GetHashCode() ^ DispersionFactorPerShot.GetHashCode();

        #endregion
    }
}