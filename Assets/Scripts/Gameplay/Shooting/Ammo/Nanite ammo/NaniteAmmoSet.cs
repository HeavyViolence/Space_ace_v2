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
    public sealed class NaniteAmmoSet : AmmoSet, IEquatable<NaniteAmmoSet>
    {
        private readonly Nanites _nanites;

        public float DamagePerSecond => _nanites.DamagePerSecond;
        public float DamageDuration => _nanites.DamageDuration;

        protected override ShotBehaviourAsync ShotBehaviourAsync => async delegate (object user, Gun gun)
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
                HitBehaviour?.Invoke(hitArgs);
                Services.ProjectileFactory.Release(projectile, ProjectileSkin);
                Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, hitArgs.HitPosition).Forget();
            };

            projectile.Escapable.WaitForEscapeAsync().Forget();

            projectile.Escapable.Escaped += (sender, args) =>
            {
                MissBehaviour?.Invoke();
                Services.ProjectileFactory.Release(projectile, ProjectileSkin);
            };

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.transform.position, null, true).Forget();

            Amount--;
            Price -= ShotPrice;

            await UniTask.WaitForSeconds(1f / gun.FireRate);

            return new(1, HeatGeneration);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            Vector2 velocity = data.InitialVelocity * Time.fixedDeltaTime;
            body.MovePosition(body.position + velocity);
        };

        protected override HitBehaviour HitBehaviour => delegate (HitEventArgs hitArgs)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);

            if (hitArgs.ObjectBeingHit.TryGetComponent(out INaniteTarget target) == true)
            {
                target.TryApplyNanitesAsync(_nanites).Forget();
            }
        };

        protected override MissBehaviour MissBehaviour => null;

        public NaniteAmmoSet(AmmoServices services,
                             NaniteAmmoSetConfig config,
                             NaniteAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            _nanites = new(savedState.DamagePerSecond, savedState.DamageDuration);
        }

        public NaniteAmmoSet(AmmoServices services,
                             Size size,
                             Quality quality,
                             NaniteAmmoSetConfig config) : base(services, size, quality, config)
        {
            float damagePerSecond = services.ItemPropertyEvaluator.Evaluate(config.DamagePerSecond, true, quality, size, SizeInfluence.Direct);
            float damageDuration = services.ItemPropertyEvaluator.Evaluate(config.DamageDuration, true, quality, size, SizeInfluence.Direct);

            _nanites = new(damagePerSecond, damageDuration);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Nanite/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Nanite/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Nanite/Code", this);

        public override ItemSavableState GetSavableState() =>
            new NaniteAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, DamagePerSecond, DamageDuration);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as NaniteAmmoSet);

        public bool Equals(NaniteAmmoSet other) => other is not null &&
                                                   DamagePerSecond == other.DamageDuration &&
                                                   DamageDuration == other.DamageDuration;

        public override int GetHashCode() => base.GetHashCode() ^
                                             DamagePerSecond.GetHashCode() ^
                                             DamageDuration.GetHashCode();

        #endregion
    }
}