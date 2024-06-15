using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Players;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories.ProjectileFactories;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class HomingAmmoSet : AmmoSet, IEquatable<HomingAmmoSet>
    {
        public float HomingSpeed { get; }
        public float TargetingWidth { get; }

        public HomingAmmoSet(AmmoServices services,
                             HomingAmmoSetConfig config,
                             HomingAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            HomingSpeed = savedState.HomingSpeed;
            TargetingWidth = savedState.TargetingWidth;
        }

        public HomingAmmoSet(AmmoServices services,
                             Size size,
                             Quality quality,
                             HomingAmmoSetConfig config) : base(services, size, quality, config)
        {
            HomingSpeed = services.ItemPropertyEvaluator.Evaluate(config.HomingSpeed,
                                                                  RangeEvaluationDirection.Forward,
                                                                  quality,
                                                                  size,
                                                                  SizeInfluence.None);

            TargetingWidth = services.ItemPropertyEvaluator.Evaluate(config.TargetingWidth,
                                                                     RangeEvaluationDirection.Backward,
                                                                     quality,
                                                                     size,
                                                                     SizeInfluence.None);
        }

        public override async UniTask FireAsync(object shooter,
                                                IGun gun,
                                                CancellationToken fireCancellation = default,
                                                CancellationToken overheatCancellation = default)
        {
            if (shooter is null) throw new ArgumentNullException();
            if (gun is null) throw new ArgumentNullException();

            while (Amount > 0 && fireCancellation.IsCancellationRequested == false && overheatCancellation.IsCancellationRequested == false)
            {
                if (AuxMath.RandomNormal < EMPFactor)
                {
                    await UniTask.WaitUntil(() => Services.GamePauser.Paused == false);
                    await UniTask.WaitForSeconds(1f / gun.FireRate);

                    continue;
                }

                ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

                float dispersion = AuxMath.RandomUnit * gun.Dispersion;

                Vector2 projectileDirection = new(gun.Transform.up.x + gun.SignedConvergenceAngle + dispersion, gun.Transform.up.y);
                projectileDirection.Normalize();

                Quaternion projectileRotation = gun.Transform.rotation * Quaternion.Euler(0f, 0f, gun.SignedConvergenceAngle + dispersion);
                projectileRotation.Normalize();

                Vector2 boxSize = new(TargetingWidth, TargetingWidth);
                Vector2 boxDirection = new(gun.Transform.up.x, gun.Transform.up.y);

                int layerMask;

                if (shooter is Player)
                {
                    layerMask = PlayerTargetsMask;
                    Amount--;
                }
                else
                {
                    layerMask = EnemyTargetsMask;
                }

                RaycastHit2D hit = Physics2D.BoxCast(gun.Transform.position, boxSize, 0f, boxDirection, float.PositiveInfinity, layerMask);
                Transform target = hit.transform;

                MovementData data = new(Speed, Speed, 0f, gun.Transform.position, projectileDirection, projectileRotation, target, HomingSpeed, 0f);

                projectile.Transform.SetPositionAndRotation(gun.Transform.position, projectileRotation);
                projectile.MovementBehaviourSupplier.Supply(OnMove, data);

                projectile.DamageDealer.Hit += (_, e) =>
                {
                    OnHit(shooter, e);
                    Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                    Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, e.HitPosition).Forget();
                };

                projectile.Escapable.Escaped += (_, _) => Services.ProjectileFactory.Release(ProjectileSkin, projectile);

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();
                if (gun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFired();

                OnShotFired(HeatGeneration);

                await UniTask.WaitUntil(() => Services.GamePauser.Paused == false);
                await UniTask.WaitForSeconds(1f / gun.FireRate);
            }

            ClearOnShotFired();
        }

        protected override void OnMove(Rigidbody2D body, MovementData data)
        {
            if (data.CurrentDirection == Vector3.zero) data.CurrentDirection = data.InitialDirection;

            if (data.Target == null)
            {
                body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
            }
            else
            {
                if (data.CurrentRotation == Quaternion.identity)
                    data.CurrentRotation = Quaternion.LookRotation(Vector3.forward, data.InitialDirection).normalized;

                if (data.Target.gameObject.activeInHierarchy == true)
                {
                    Vector3 targetDirection = (data.Target.position - body.transform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, targetDirection).normalized;

                    data.CurrentRotation = Quaternion.RotateTowards(data.CurrentRotation, targetRotation, data.HomingSpeed * Time.fixedDeltaTime);
                    data.CurrentDirection = Vector3.RotateTowards(data.CurrentDirection,
                                                                  targetDirection,
                                                                  data.HomingSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime,
                                                                  1f).normalized;

                    body.MovePosition(body.position + data.CurrentVelocityPerFixedUpdate);
                    body.MoveRotation(data.CurrentRotation);
                }
                else
                {
                    body.MovePosition(body.position + data.CurrentVelocityPerFixedUpdate);
                }
            }
        }

        protected override void OnHit(object shooter, HitEventArgs e, float damageFactor = 1f)
        {
            e.Damageable.ApplyDamage(Damage);
        }

        protected override void OnMiss(object shooter) { }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Homing/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Homing/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Homing/Code", this);

        public override ItemSavableState GetSavableState() =>
            new HomingAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, HomingSpeed, TargetingWidth);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as HomingAmmoSet);

        public bool Equals(HomingAmmoSet other) => other is not null &&
                                                   HomingSpeed == other.HomingSpeed &&
                                                   TargetingWidth == other.TargetingWidth;

        public override int GetHashCode() => base.GetHashCode() ^
                                             HomingSpeed.GetHashCode() ^
                                             TargetingWidth.GetHashCode();

        #endregion
    }
}