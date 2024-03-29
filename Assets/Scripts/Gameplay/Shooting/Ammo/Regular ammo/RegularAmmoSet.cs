using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class RegularAmmoSet : AmmoSet
    {
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

        protected override HitBehaviour HitBehaviour => delegate (HitEventArgs hitArgs, object[] args)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);
        };

        protected override MissBehaviour MissBehaviour => null;

        public RegularAmmoSet(AmmoServices services,
                              Size size,
                              Quality quality,
                              RegularAmmoSetConfig config) : base(services, size, quality, config)
        { }

        public RegularAmmoSet(AmmoServices services,
                              RegularAmmoSetConfig config,
                              RegularAmmoSetSavableState savedState) : base(services, config, savedState)
        { }

        public override async UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Regular/Name", this);

        public override async UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Regular/Description", this);

        public override ItemSavableState GetSavableState() =>
            new RegularAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage);
    }
}