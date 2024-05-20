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
    public class MissileSet : AmmoSet, IEquatable<MissileSet>
    {
        private readonly AnimationCurve _speedGainCurve;

        public float HomingSpeed { get; }
        public float TargetingWidth { get; }
        public float SpeedGainDuration { get; }

        protected sealed override ShotBehaviourAsync ShotBehaviourAsync => async delegate (object user, IGun gun)
        {
            ProjectileCache projectile = Services.ProjectileFactory.Create(user, ProjectileSkin, Size);

            float dispersion = AuxMath.RandomUnit * gun.Dispersion;

            Vector2 projectileDirection = new(gun.Transform.up.x + gun.SignedConvergenceAngle + dispersion, gun.Transform.up.y);
            projectileDirection.Normalize();

            Quaternion projectileRotation = gun.Transform.rotation * Quaternion.Euler(0f, 0f, gun.SignedConvergenceAngle + dispersion);
            projectileRotation.Normalize();

            Vector2 boxSize = new(TargetingWidth, TargetingWidth);
            Vector2 boxDirection = new(gun.Transform.up.x, gun.Transform.up.y);

            int layerMask;

            if (user is Player)
            {
                layerMask = LayerMask.GetMask("Enemies", "Bosses", "Meteors", "Wrecks");

                Amount--;
                Price -= ShotPrice;
            }
            else
            {
                layerMask = LayerMask.GetMask("Player");
            }

            RaycastHit2D hit = Physics2D.BoxCast(gun.Transform.position, boxSize, 0f, boxDirection, float.PositiveInfinity, layerMask);
            Transform target = hit.transform;

            MovementData data = new(0f, Speed, SpeedGainDuration, gun.Transform.position, projectileDirection, projectileRotation, target, HomingSpeed, 0f, _speedGainCurve);

            projectile.Object.transform.SetPositionAndRotation(gun.Transform.position, projectileRotation);
            projectile.MovementBehaviourSupplier.Supply(MovementBehaviour, data);

            projectile.DamageDealer.Hit += (sender, hitArgs) =>
            {
                HitBehaviour?.Invoke(user, hitArgs);
                Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, hitArgs.HitPosition).Forget();
            };

            projectile.Escapable.Escaped += (sender, args) =>
            {
                MissBehaviour?.Invoke(user);
                Services.ProjectileFactory.Release(ProjectileSkin, projectile);
            };

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();

            await UniTask.WaitForSeconds(1f / gun.FireRate);

            return new(1, HeatGeneration);
        };

        protected sealed override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            data.Timer += Time.fixedDeltaTime;

            float currentSpeedEvaluator = data.SpeedGainCurve.Evaluate(data.Timer / data.FinalSpeedGainDuration);
            data.CurrentSpeed = Mathf.Lerp(data.InitialSpeed, data.FinalSpeed, currentSpeedEvaluator);

            if (data.CurrentDirection == Vector3.zero) data.CurrentDirection = data.InitialDirection;

            if (data.Target == null)
            {
                body.MovePosition(body.position + data.CurrentVelocityPerFixedUpdate);
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
        };

        protected override HitBehaviour HitBehaviour => delegate (object shooter, HitEventArgs hitArgs)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);
        };

        protected override MissBehaviour MissBehaviour => null;

        public MissileSet(AmmoServices services,
                          MissileSetConfig config,
                          MissileSetSavableState savedState) : base(services, config, savedState)
        {
            _speedGainCurve = config.SpeedGainCurve;
            HomingSpeed = savedState.HomingSpeed;
            TargetingWidth = savedState.TargetingWidth;
            SpeedGainDuration = savedState.SpeedGainDuration;
        }

        public MissileSet(AmmoServices services,
                          Size size,
                          Quality quality,
                          MissileSetConfig config) : base(services, size, quality, config)
        {
            _speedGainCurve = config.SpeedGainCurve;
            HomingSpeed = services.ItemPropertyEvaluator.Evaluate(config.HomingSpeed, true, quality, size, SizeInfluence.Inverted);
            TargetingWidth = services.ItemPropertyEvaluator.Evaluate(config.TargetingWidth, false, quality, size, SizeInfluence.None);
            SpeedGainDuration = services.ItemPropertyEvaluator.Evaluate(config.SpeedGainDuration, false, quality, size, SizeInfluence.Inverted);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Missile/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Missile/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Missile/Code", this);

        public override ItemSavableState GetSavableState() =>
            new MissileSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, HomingSpeed, TargetingWidth, SpeedGainDuration);

        #region interfaces

        public sealed override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as MissileSet);

        public virtual bool Equals(MissileSet other) => other is not null &&
                                                        HomingSpeed == other.HomingSpeed &&
                                                        TargetingWidth == other.TargetingWidth &&
                                                        SpeedGainDuration == other.SpeedGainDuration;

        public override int GetHashCode() => base.GetHashCode() ^
                                             HomingSpeed.GetHashCode() ^
                                             TargetingWidth.GetHashCode() ^
                                             SpeedGainDuration.GetHashCode();

        #endregion
    }
}