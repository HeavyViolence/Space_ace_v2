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
    public sealed class CatalyticAmmoSet : AmmoSet, IEquatable<CatalyticAmmoSet>
    {
        private float _fireRateFactor = 1f;

        public float FireRateFactorPerShot { get; }
        public float FireRateIncreasePerShotPercentage => (FireRateFactorPerShot - 1f) * 100f;

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

            if (shooter is Player)
            {
                Amount--;
                Price -= ShotPrice;
            }

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gunView.Transform.position, null, true).Forget();

            if (gunView.FirstShotInLine == true) _fireRateFactor = 1f;
            else _fireRateFactor *= FireRateFactorPerShot;

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

        public CatalyticAmmoSet(AmmoServices services,
                                CatalyticAmmoSetConfig config,
                                CatalyticAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            FireRateFactorPerShot = savedState.FirerateFactorPerShot;
        }

        public CatalyticAmmoSet(AmmoServices services,
                                Size size,
                                Quality quality,
                                CatalyticAmmoSetConfig config) : base(services, size, quality, config)
        {
            FireRateFactorPerShot = services.ItemPropertyEvaluator.Evaluate(config.FireRateFactorPerShot, true, quality, size, SizeInfluence.None);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Catalytic/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Catalytic/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Catalytic/Code", this);

        public override ItemSavableState GetSavableState() =>
            new CatalyticAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, FireRateFactorPerShot);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as CatalyticAmmoSet);

        public bool Equals(CatalyticAmmoSet other) => other is not null &&
                                                      FireRateFactorPerShot == other.FireRateFactorPerShot;

        public override int GetHashCode() => base.GetHashCode() ^ FireRateFactorPerShot.GetHashCode();

        #endregion
    }
}