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
    public sealed class ExplosiveAmmoSet : AmmoSet, IEquatable<ExplosiveAmmoSet>
    {
        public float ExplosionRadius { get; }
        public float ExplosionDamage { get; }

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
            hitArgs.DamageReceiver.ApplyDamage(Damage);

            RaycastHit2D[] hits = Physics2D.CircleCastAll(hitArgs.HitPosition, ExplosionRadius, Vector2.zero);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject.TryGetComponent(out IDamageable damageReceiver) == true)
                {
                    float explosionDamageFactor = Mathf.Lerp(ExplosionDamage, 0f, hit.distance / ExplosionRadius);
                    float explosionDamage = ExplosionDamage * explosionDamageFactor;

                    damageReceiver.ApplyDamage(explosionDamage);
                }
            }
        };

        protected override MissBehaviour MissBehaviour => null;

        public ExplosiveAmmoSet(AmmoServices services,
                                ExplosiveAmmoSetConfig config,
                                ExplosiveAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            ExplosionRadius = savedState.ExplosionRadius;
            ExplosionDamage = savedState.ExplosionDamage;
        }

        public ExplosiveAmmoSet(AmmoServices services,
                                Size size,
                                Quality quality,
                                ExplosiveAmmoSetConfig config) : base(services, size, quality, config)
        {
            ExplosionRadius = services.ItemPropertyEvaluator.Evaluate(config.ExplosionRadius, true, quality, size, SizeInfluence.Direct);
            ExplosionDamage = services.ItemPropertyEvaluator.Evaluate(config.ExplosionDamage, true, quality, size, SizeInfluence.Direct);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Explosive/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Explosive/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Explosive/Code", this);

        public override ItemSavableState GetSavableState() =>
            new ExplosiveAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, ExplosionRadius, ExplosionDamage);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as ExplosiveAmmoSet);

        public bool Equals(ExplosiveAmmoSet other) => other is not null &&
                                                      ExplosionRadius == other.ExplosionRadius &&
                                                      ExplosionDamage == other.ExplosionDamage;

        public override int GetHashCode() => base.GetHashCode() ^
                                             ExplosionRadius.GetHashCode() ^
                                             ExplosionDamage.GetHashCode();

        #endregion
    }
}