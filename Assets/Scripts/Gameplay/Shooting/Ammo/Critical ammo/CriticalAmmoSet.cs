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

        protected override ShotBehaviourAsync ShotBehaviourAsync => async delegate (object shooter, IGun gun)
        {
            ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

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

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();

            await UniTask.WaitForSeconds(1f / gun.FireRate);

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