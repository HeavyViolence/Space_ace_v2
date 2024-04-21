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
    public sealed class EntangledAmmoSet : AmmoSet, IEquatable<EntangledAmmoSet>
    {
        public int AmmoLossOnMiss { get; }
        public int AmmoGainOnHit { get; }

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

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.transform.position, null, true).Forget();

            await UniTask.WaitForSeconds(1f / gun.FireRate);

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

            Amount += AmmoGainOnHit;
            Price += ShotPrice * AmmoGainOnHit;
        };

        protected override MissBehaviour MissBehaviour => delegate (object[] args)
        {
            Amount -= AmmoLossOnMiss;
            Price -= ShotPrice * AmmoLossOnMiss;
        };

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