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
    public sealed class ClusterAmmoSet : AmmoSet, IEquatable<ClusterAmmoSet>
    {
        public int ProjectilesPerShot { get; }

        protected override ShotBehaviourAsync ShotBehaviourAsync => async delegate (object user, IGun gun)
        {
            for (int i = 0; i < ProjectilesPerShot; i++)
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
                    Services.ProjectileFactory.Release(projectile, ProjectileSkin);
                    Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, hitArgs.HitPosition).Forget();
                };

                projectile.Escapable.Escaped += (sender, args) =>
                {
                    MissBehaviour?.Invoke();
                    Services.ProjectileFactory.Release(projectile, ProjectileSkin);
                };
            }

            Amount--;
            Price -= ShotPrice;

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();

            await UniTask.WaitForSeconds(1f / gun.FireRate);

            return new(ProjectilesPerShot, HeatGeneration);
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