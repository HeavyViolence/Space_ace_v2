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
    public sealed class CriticalAmmoSet : AmmoSet, IEquatable<CriticalAmmoSet>
    {
        public float CriticalDamageProbability { get; }
        public float CriticalDamageProbabilityPercentage => CriticalDamageProbability * 100f;
        public float CriticalDamage { get; }

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

            return new(1, HeatGeneration);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        protected override HitBehaviour HitBehaviour => delegate (object shooter, HitEventArgs hitArgs)
        {
            if (AuxMath.RandomNormal < CriticalDamageProbability) hitArgs.DamageReceiver.ApplyDamage(CriticalDamage);
            else hitArgs.DamageReceiver.ApplyDamage(Damage);
        };

        protected override MissBehaviour MissBehaviour => null;

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