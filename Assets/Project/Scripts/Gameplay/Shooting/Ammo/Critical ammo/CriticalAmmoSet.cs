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

        public override async UniTask FireAsync(object shooter,
                                                IGun gun,
                                                CancellationToken fireCancellation = default,
                                                CancellationToken overheatCancellation = default)
        {
            if (shooter is null) throw new ArgumentNullException();
            if (gun is null) throw new ArgumentNullException();

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
                    Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                    Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, e.HitPosition).Forget();
                };

                projectile.Escapable.Escaped += (_, _) => Services.ProjectileFactory.Release(ProjectileSkin, projectile);

                if (shooter is Player) Amount--;

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();
                if (gun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFired();

                OnShotFired(HeatGeneration);

                await UniTask.WaitUntil(() => Services.GamePauser.Paused == false);
                await UniTask.WaitForSeconds(1f / gun.FireRate);
            }

            ClearOnShotFired();
        }

        protected override void OnMove(Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        }

        protected override void OnHit(object shooter, HitEventArgs e)
        {
            if (AuxMath.RandomNormal < CriticalDamageProbability) e.Damageable.ApplyDamage(CriticalDamage);
            else e.Damageable.ApplyDamage(Damage);
        }

        protected override void OnMiss(object shooter) { }

        public CriticalAmmoSet(AmmoServices services,
                               Size size,
                               Quality quality,
                               CriticalAmmoSetConfig config) : base(services, size, quality, config)
        {
            CriticalDamageProbability = services.ItemPropertyEvaluator.Evaluate(config.CriticalDamageProbability, true, quality, size, SizeInfluence.None);
            CriticalDamage = services.ItemPropertyEvaluator.Evaluate(config.CriticalDamage, true, quality, size, SizeInfluence.Direct);
        }

        public CriticalAmmoSet(AmmoServices services,
                               CriticalAmmoSetConfig config,
                               CriticalAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            CriticalDamageProbability = savedState.CriticalDamageProbability;
            CriticalDamage = savedState.CriticalDamage;
        }

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