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
    public sealed class StrangeAmmoSet : AmmoSet, IEquatable<StrangeAmmoSet>
    {
        public float AmmoLossProbability { get; }
        public float AmmoLossProbabilityPercentage => AmmoLossProbability * 100f;

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

            if (shooter is Player && AuxMath.RandomNormal < AmmoLossProbability)
            {
                Amount--;
                Price -= ShotPrice;
            }

            return new(1, HeatGeneration);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        protected override HitBehaviour HitBehaviour => delegate (object shooter, HitEventArgs hitArgs)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);
        };

        protected override MissBehaviour MissBehaviour => null;

        public StrangeAmmoSet(AmmoServices services,
                              Size size,
                              Quality quality,
                              StrangeAmmoSetConfig config) : base(services, size, quality, config)
        {
            AmmoLossProbability = services.ItemPropertyEvaluator.Evaluate(config.AmmoLossProbability, false, quality, size, SizeInfluence.None);
        }

        public StrangeAmmoSet(AmmoServices services,
                              StrangeAmmoSetConfig config,
                              StrangeAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            AmmoLossProbability = savedState.AmmoLossProbability;
        }

        public override async UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Strange/Name", this);

        public override async UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Strange/Description", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Strange/Code", this);

        public override ItemSavableState GetSavableState() =>
            new StrangeAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, AmmoLossProbability);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as StrangeAmmoSet);

        public bool Equals(StrangeAmmoSet other) => other is not null &&
                                                    AmmoLossProbability == other.AmmoLossProbability;

        public override int GetHashCode() => base.GetHashCode() ^ AmmoLossProbability.GetHashCode();

        #endregion
    }
}