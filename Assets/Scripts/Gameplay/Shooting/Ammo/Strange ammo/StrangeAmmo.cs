using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Inventories;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class StrangeAmmo : Ammo
    {
        public override AmmoType Type => AmmoType.Strange;

        protected override Func<Gun, CancellationToken, UniTask> ShotBehaviour =>
            async delegate (Gun gun, CancellationToken token)
            {
                while (token.IsCancellationRequested == false)
                {
                    CachedProjectile projectile = Services.ProjectileFactory.Create(gun.ProjectileRequestor, Skin);

                    float dispersion = AuxMath.RandomUnit * gun.Dispersion;

                    Vector2 projectileDirection = new(gun.SignedConvergenceAngle + dispersion, gun.transform.up.y);
                    projectileDirection.Normalize();

                    Quaternion projectileRotation = gun.transform.rotation * Quaternion.Euler(0f, 0f, dispersion);
                    projectileRotation.Normalize();

                    MovementData movementData = new(Speed, Speed, 0f, gun.transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

                    projectile.Instance.transform.SetPositionAndRotation(gun.transform.position, projectileRotation);
                    projectile.MovementBehaviourSupplier.Supply((Rigidbody2D body, ref MovementData data) =>
                    {
                        Vector2 velocity = data.InitialSpeed * Time.fixedDeltaTime * data.InitialVelocity;
                        body.MovePosition(body.position + velocity);
                    }, movementData);

                    projectile.DamageDealer.Hit += (sender, args) =>
                    {
                        args.DamageReceiver.ApplyDamage(Damage);
                        Services.ProjectileFactory.Release(projectile, Skin);
                    };

                    projectile.Escapable.WaitForEscapeAsync(() =>
                    Services.MasterCameraHolder.InsideViewport(projectile.Instance.transform.position), ProjectileReleaseDelay).Forget();

                    projectile.Escapable.Escaped += (sender, args) =>
                    {
                        Services.ProjectileFactory.Release(projectile, Skin);
                    };

                    RaiseShotEvent(0, HeatGeneration);

                    while (Services.GamePauser.Paused == true) await UniTask.Yield();

                    await UniTask.WaitForSeconds(1f / gun.FireRate, false, PlayerLoopTiming.Update, token);
                }
            };

        public StrangeAmmo(AmmoServices services,
                           ItemSize size,
                           ItemQuality quality,
                           ProjectileSkin skin,
                           StrangeAmmoConfig config) : base(services,
                                                            size,
                                                            quality,
                                                            skin,
                                                            config)
        { }

        public override async UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Strange.Name", this);

        public override async UniTask<string> GetStatsAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Strange.Description", this);
    }
}