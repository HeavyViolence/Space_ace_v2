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
    public sealed class CriticalAmmoSet : AmmoSet, IEquatable<CriticalAmmoSet>
    {
        public float CriticalDamageProbability { get; }
        public float CriticalDamageProbabilityPercentage => CriticalDamageProbability * 100f;
        public float CriticalDamage { get; }

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

            Amount--;
            Price -= ShotPrice;

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