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
    public sealed class ClusterAmmoSet : AmmoSet, IEquatable<ClusterAmmoSet>
    {
        public int ProjectilesPerShot { get; }

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

                for (int i = 0; i < ProjectilesPerShot; i++)
                {
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
                }

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
            e.Damageable.ApplyDamage(Damage);
        }

        protected override void OnMiss(object shooter) { }

        public ClusterAmmoSet(AmmoServices services,
                              Size size,
                              Quality quality,
                              ClusterAmmoSetConfig config) : base(services, size, quality, config)
        {
            ProjectilesPerShot = services.ItemPropertyEvaluator.Evaluate(config.ProjectilesPerShot, true, quality, size, SizeInfluence.Direct);
        }

        public ClusterAmmoSet(AmmoServices services,
                              ClusterAmmoSetConfig config,
                              ClusterAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            ProjectilesPerShot = savedState.ProjectilesPerShot;
        }

        public override async UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Cluster/Description", this);

        public override async UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Cluster/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Cluster/Code", this);

        public override ItemSavableState GetSavableState() =>
            new ClusterAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, ProjectilesPerShot);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as ClusterAmmoSet);

        public bool Equals(ClusterAmmoSet other) => other is not null &&
                                                    ProjectilesPerShot == other.ProjectilesPerShot;

        public override int GetHashCode() => base.GetHashCode() ^ ProjectilesPerShot.GetHashCode();

        #endregion
    }
}