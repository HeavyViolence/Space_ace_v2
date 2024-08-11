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
    public sealed class CriticalAmmoSet : AmmoSet, IEquatable<CriticalAmmoSet>
    {
        public float CriticalDamageProbability { get; }
        public float CriticalDamageProbabilityPercentage => CriticalDamageProbability * 100f;
        public float CriticalDamage { get; }

        public CriticalAmmoSet(AmmoServices services,
                               Size size,
                               Quality quality,
                               CriticalAmmoSetConfig config) : base(services, size, quality, config)
        {
            CriticalDamageProbability = services.ItemPropertyEvaluator.Evaluate(config.CriticalDamageProbability,
                                                                                RangeEvaluationDirection.Forward,
                                                                                quality,
                                                                                size,
                                                                                SizeInfluence.None);

            CriticalDamage = services.ItemPropertyEvaluator.Evaluate(config.CriticalDamage,
                                                                     RangeEvaluationDirection.Forward,
                                                                     quality,
                                                                     size,
                                                                     SizeInfluence.Direct);
        }

        public CriticalAmmoSet(AmmoServices services,
                               CriticalAmmoSetConfig config,
                               CriticalAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            CriticalDamageProbability = savedState.CriticalDamageProbability;
            CriticalDamage = savedState.CriticalDamage;
        }

        public override async UniTask FireAsync(object shooter, IGun gun, CancellationToken token)
        {
            if (shooter is null) throw new ArgumentNullException();
            if (gun is null) throw new ArgumentNullException();

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
                    OnHit(shooter, e);
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
            if (AuxMath.RandomNormal < CriticalDamageProbability) e.Damageable.ApplyDamage(CriticalDamage);
            else e.Damageable.ApplyDamage(Damage);
        }

        protected override void OnMiss(object shooter) { }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Critical/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Critical/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Critical/Code", this);

        public override ItemSavableState GetSavableState() =>
            new CriticalAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, CriticalDamageProbability, CriticalDamage);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as CriticalAmmoSet);

        public bool Equals(CriticalAmmoSet other) => other is not null &&
                                                     CriticalDamageProbability == other.CriticalDamageProbability &&
                                                     CriticalDamage == other.CriticalDamage;

        public override int GetHashCode() => base.GetHashCode() ^
                                             CriticalDamageProbability.GetHashCode() ^
                                             CriticalDamage.GetHashCode();

        #endregion
    }
}