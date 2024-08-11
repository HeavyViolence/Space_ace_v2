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
    public sealed class DevastatingAmmoSet : AmmoSet, IEquatable<DevastatingAmmoSet>
    {
        public float ConsecutiveDamageFactor { get; }
        public float DamageIncreasePerHitPercentage => (ConsecutiveDamageFactor - 1f) * 100f;

        public DevastatingAmmoSet(AmmoServices services,
                                  DevastatingAmmoSetConfig config,
                                  DevastatingAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            ConsecutiveDamageFactor = savedState.ConsecutiveDamageFactor;
        }

        public DevastatingAmmoSet(AmmoServices services,
                                  Size size,
                                  Quality quality,
                                  DevastatingAmmoSetConfig config) : base(services, size, quality, config)
        {
            ConsecutiveDamageFactor = services.ItemPropertyEvaluator.Evaluate(config.ConsecutiveDamegeFactor,
                                                                              RangeEvaluationDirection.Forward,
                                                                              quality,
                                                                              size,
                                                                              SizeInfluence.None);
        }

        public override async UniTask FireAsync(object shooter, IGun gun, CancellationToken token)
        {
            if (shooter is null) throw new ArgumentNullException();
            if (gun is null) throw new ArgumentNullException();

            Guid previourTargetID  = Guid.Empty;
            float damageFactor = 1f;

            while (Amount > 0 && token.IsCancellationRequested == false)
            {
                if (AuxMath.RandomNormal < EMPFactor)
                {
                    await AuxAsync.DelayAsync(() => 1f / gun.FireRate, () => Services.GamePauser.Paused == true, token);
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
                    if (e.Damageable.ID == previourTargetID)
                    {
                        damageFactor *= ConsecutiveDamageFactor;
                    }
                    else
                    {
                        damageFactor = 1f;
                        previourTargetID = e.Damageable.ID;
                    }

                    OnHit(shooter, e, damageFactor);
                    Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                    Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, e.HitPosition).Forget();
                };

                projectile.Escapable.Escaped += (_, _) => Services.ProjectileFactory.Release(ProjectileSkin, projectile);

                if (shooter is Player) Amount--;

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();
                if (gun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFired();

                OnShotFired(HeatGeneration);

                await AuxAsync.DelayAsync(() => 1f / gun.FireRate, () => Services.GamePauser.Paused == true, token);
            }

            ClearOnShotFired();
        }

        protected override void OnMove(Rigidbody2D body, MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        }

        protected override void OnHit(object shooter, HitEventArgs e, float damageFactor = 1f)
        {
            e.Damageable.ApplyDamage(Damage * damageFactor);
        }

        protected override void OnMiss(object shooter) { }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Devastating/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Devastating/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Devastating/Code", this);

        public override ItemSavableState GetSavableState() =>
            new DevastatingAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, ConsecutiveDamageFactor);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as DevastatingAmmoSet);

        public bool Equals(DevastatingAmmoSet other) => other is not null &&
                                                        ConsecutiveDamageFactor == other.ConsecutiveDamageFactor;

        public override int GetHashCode() => base.GetHashCode() ^ ConsecutiveDamageFactor.GetHashCode();

        #endregion
    }
}
