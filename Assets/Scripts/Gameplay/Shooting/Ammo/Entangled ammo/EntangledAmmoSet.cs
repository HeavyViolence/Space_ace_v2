using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories.ProjectileFactories;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class EntangledAmmoSet : AmmoSet, IEquatable<EntangledAmmoSet>
    {
        public int AmmoLossOnMiss { get; }
        public int AmmoGainOnHit { get; }

        public override ShotBehaviour ShotBehaviour => delegate (object shooter, IGunView gunView)
        {
            ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

            float dispersion = AuxMath.RandomUnit * gunView.Dispersion;

            Vector2 projectileDirection = new(gunView.Transform.up.x + gunView.SignedConvergenceAngle + dispersion, gunView.Transform.up.y);
            projectileDirection.Normalize();

            Quaternion projectileRotation = gunView.Transform.rotation * Quaternion.Euler(0f, 0f, gunView.SignedConvergenceAngle + dispersion);
            projectileRotation.Normalize();

            MovementData data = new(Speed, Speed, 0f, gunView.Transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

            projectile.Transform.SetPositionAndRotation(gunView.Transform.position, projectileRotation);
            projectile.MovementBehaviourSupplier.Supply(MovementBehaviour, data);

            projectile.DamageDealer.Hit += (sender, hitArgs) =>
            {
                HitBehaviour?.Invoke(shooter, hitArgs);
                Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, hitArgs.HitPosition).Forget();
            };

            projectile.Escapable.Escaped += (sender, args) =>
            {
                MissBehaviour?.Invoke(shooter);
                Services.ProjectileFactory.Release(ProjectileSkin, projectile);
            };

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gunView.Transform.position, null, true).Forget();

            return new(1, HeatGeneration);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        protected override HitBehaviour HitBehaviour => delegate (object shooter, HitEventArgs hitArgs)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);

            if (shooter is Player)
            {
                Amount += AmmoGainOnHit;
                Price += ShotPrice * AmmoGainOnHit;
            }
        };

        protected override MissBehaviour MissBehaviour => delegate (object shooter)
        {
            if (shooter is not Player) return;

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