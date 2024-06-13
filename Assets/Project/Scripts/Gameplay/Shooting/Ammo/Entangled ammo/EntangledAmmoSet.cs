using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main;
using SpaceAce.Main.Factories.ProjectileFactories;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class EntangledAmmoSet : AmmoSet, IEquatable<EntangledAmmoSet>
    {
        public int AmmoLossOnMiss { get; }
        public int AmmoGainOnHit { get; }

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

                projectile.Escapable.Escaped += (_, _) =>
                {
                    OnMiss(shooter);
                    Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                };

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
            e.Damageable.ApplyDamage(Damage);
            if (shooter is Player) Amount += AmmoGainOnHit;
        }

        protected override void OnMiss(object shooter)
        {
            if (shooter is not Player && Services.GameStateLoader.CurrentState != GameState.Level) return;
            Amount -= AmmoLossOnMiss;
        }

        public EntangledAmmoSet(AmmoServices services,
                                EntangledAmmoSetConfig config,
                                EntangledAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            AmmoLossOnMiss = savedState.AmmoLossOnMiss;
            AmmoGainOnHit = savedState.AmmoGainOnHit;
        }

        public EntangledAmmoSet(AmmoServices services,
                                Size size,
                                Quality quality,
                                EntangledAmmoSetConfig config) : base(services, size, quality, config)
        {
            AmmoLossOnMiss = services.ItemPropertyEvaluator.Evaluate(config.AmmoLossOnMiss, false, quality, size, SizeInfluence.None);
            AmmoGainOnHit = services.ItemPropertyEvaluator.Evaluate(config.AmmoGainOnHit, true, quality, size, SizeInfluence.None);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Entangled/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Entangled/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Entangled/Code", this);

        public override ItemSavableState GetSavableState() =>
            new EntangledAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, AmmoLossOnMiss, AmmoGainOnHit);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as EntangledAmmoSet);

        public bool Equals(EntangledAmmoSet other) => other is not null &&
                                                      AmmoLossOnMiss == other.AmmoLossOnMiss &&
                                                      AmmoGainOnHit == other.AmmoGainOnHit;

        public override int GetHashCode() => base.GetHashCode() ^
                                             AmmoLossOnMiss.GetHashCode() ^
                                             AmmoGainOnHit.GetHashCode();

        #endregion
    }
}