using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories;

using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class RegularAmmoSet : AmmoSet
    {
        public override AmmoType Type => AmmoType.Regular;

        protected override ShootingBehaviour ShootingBehaviour => async delegate (Gun gun, CancellationToken token, object[] args)
        {
            while (Amount > 0 && token.IsCancellationRequested == false)
            {
                CachedProjectile projectile = Services.ProjectileFactory.Create(gun.ProjectileRequestor, ProjectileSkin);

                float dispersion = AuxMath.RandomUnit * gun.Dispersion;

                Vector2 projectileDirection = new(gun.SignedConvergenceAngle + dispersion, gun.transform.up.y);
                projectileDirection.Normalize();

                Quaternion projectileRotation = gun.transform.rotation * Quaternion.Euler(0f, 0f, dispersion);
                projectileRotation.Normalize();

                MovementData movementData = new(Speed, Speed, 0f, gun.transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

                projectile.Instance.transform.SetPositionAndRotation(gun.transform.position, projectileRotation);
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

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.transform.position, null, token, true).Forget();
                if (gun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFiredAsync(token).Forget();

                RaiseShotEvent(HeatGeneration);
                UpdatePrice();
                Amount--;

                while (Services.GamePauser.Paused == true) await UniTask.Yield();

                await UniTask.WaitForSeconds(1f / gun.FireRate, false, PlayerLoopTiming.Update, token);
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

        public RegularAmmoSet(AmmoServices services,
                              Size size,
                              Quality quality,
                              RegularAmmoSetConfig config) : base(services,
                                                                  size,
                                                                  quality,
                                                                  config)
        { }

        public RegularAmmoSet(AmmoServices services,
                              RegularAmmoSetConfig config,
                              RegularAmmoSetSavableState savedState) : base(services,
                                                                            config,
                                                                            savedState)
        { }

        public override async UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Regular.Name", this);

        public override async UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Regular.Description", this);

        public override ItemSavableState GetSavableState() =>
            new RegularAmmoSetSavableState(Type, Size, Quality, Price, Amount, HeatGeneration, Speed, Damage);
    }
}