using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Inventories;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class RegularAmmo : Ammo
    {
        public override AmmoType Type => AmmoType.Regular;

        protected override ShootingBehaviour ShootingBehaviour => async delegate (CancellationToken token, object[] args)
        {
            Gun firingGun = null;
            AmmoStack ammoStack = null;

            foreach (var item in args)
            {
                if (item is Gun gun) firingGun = gun;
                if (item is AmmoStack stack) ammoStack = stack;
            }

            if (firingGun == null) throw new ArgumentNullException();
            if (ammoStack is null) throw new ArgumentNullException();

            while (ammoStack.Amount > 0 && token.IsCancellationRequested == false)
            {
                CachedProjectile projectile = Services.ProjectileFactory.Create(firingGun.ProjectileRequestor, ProjectileSkin);

                float dispersion = AuxMath.RandomUnit * firingGun.Dispersion;

                Vector2 projectileDirection = new(firingGun.SignedConvergenceAngle + dispersion, firingGun.transform.up.y);
                projectileDirection.Normalize();

                Quaternion projectileRotation = firingGun.transform.rotation * Quaternion.Euler(0f, 0f, dispersion);
                projectileRotation.Normalize();

                MovementData movementData = new(Speed, Speed, 0f, firingGun.transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

                projectile.Instance.transform.SetPositionAndRotation(firingGun.transform.position, projectileRotation);
                projectile.MovementBehaviourSupplier.Supply(MovementBehaviour, movementData);

                projectile.DamageDealer.Hit += (sender, hitArgs) =>
                {
                    HitBehaviour?.Invoke(hitArgs, args);

                    Services.ProjectileFactory.Release(projectile, ProjectileSkin);
                    Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, hitArgs.HitPosition, token).Forget();
                };

                projectile.Escapable.WaitForEscapeAsync(() =>
                Services.MasterCameraHolder.InsideViewport(projectile.Instance.transform.position), ProjectileReleaseDelay).Forget();

                projectile.Escapable.Escaped += (sender, args) =>
                {
                    MissBehaviour?.Invoke(args);

                    Services.ProjectileFactory.Release(projectile, ProjectileSkin);
                };

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, firingGun.transform.position, null, token, true).Forget();
                if (firingGun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFiredAsync(token).Forget();

                RaiseShotEvent(HeatGeneration);
                ammoStack.Remove(1);

                while (Services.GamePauser.Paused == true) await UniTask.Yield();

                await UniTask.WaitForSeconds(1f / firingGun.FireRate, false, PlayerLoopTiming.Update, token);
            }
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            Vector2 velocity = data.InitialSpeed * Time.fixedDeltaTime * data.InitialVelocity;
            body.MovePosition(body.position + velocity);
        };

        protected override HitBehaviour HitBehaviour => delegate (HitEventArgs hitArgs, object[] args)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);
        };

        protected override MissBehaviour MissBehaviour => null;

        public RegularAmmo(AmmoServices services,
                           ItemSize size,
                           ItemQuality quality,
                           ProjectileSkin projectileSkin,
                           ProjectileHitEffectSkin hitEffectSkin,
                           RegularAmmoConfig config) : base(services,
                                                            size,
                                                            quality,
                                                            projectileSkin,
                                                            hitEffectSkin,
                                                            config)
        { }

        public override async UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Regular.Name", this);

        public override async UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Regular.Description", this);
    }
}