using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Effects;
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
    public sealed class NaniteAmmoSet : AmmoSet, IEquatable<NaniteAmmoSet>
    {
        private readonly Nanites _nanites;

        public float DamagePerSecond => _nanites.DamagePerSecond;
        public float DamageDuration => _nanites.Duration;

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
            float damagePerSecond = services.ItemPropertyEvaluator.Evaluate(config.DamagePerSecond,
                                                                            RangeEvaluationDirection.Forward,
                                                                            quality,
                                                                            size,
                                                                            SizeInfluence.Direct);

            float damageDuration = services.ItemPropertyEvaluator.Evaluate(config.DamageDuration,
                                                                           RangeEvaluationDirection.Forward,
                                                                           quality,
                                                                           size,
                                                                           SizeInfluence.Direct);

            _nanites = new(damagePerSecond, damageDuration);
        }

        public override async UniTask FireAsync(object shooter, IGun gun, CancellationToken token)
        {
            if (shooter is null) throw new ArgumentNullException();
            if (gun is null) throw new ArgumentNullException();

            while (Amount > 0 && token.IsCancellationRequested == false)
            {
                if (AuxMath.RandomNormal < EMPFactor)
                {
                    await AuxAsync.DelayAsync(() => 1f / gun.FireRate, () => Services.GamePauser.Paused == true, token);
                    continue;
                }

                ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

                float dispersion = AuxMath.RandomUnit * gun.Dispersion;

                Vector2 projectileDirection = new(gun.Transform.up.x + gun.SignedConvergenceAngle + dispersion, gun.Transform.up.y);
                projectileDirection.Normalize();

                Quaternion projectileRotation = gun.Transform.rotation * Quaternion.Euler(0f, 0f, gun.SignedConvergenceAngle + dispersion);
                projectileRotation.Normalize();

                MovementData data = new(Speed, Speed, 0f, gun.Transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

                projectile.Transform.SetPositionAndRotation(gun.Transform.position, projectileRotation);
                projectile.MovementBehaviourSupplier.Supply(OnMove, data);

                projectile.DamageDealer.Hit += (_, e) =>
                {
                    OnHit(shooter, e);
                    Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                    Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, e.HitPosition).Forget();
                };

                projectile.Escapable.Escaped += (_, _) => Services.ProjectileFactory.Release(ProjectileSkin, projectile);

                if (shooter is Player) Amount--;

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();
                if (gun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFired();

                OnShotFired(HeatGeneration);

                await AuxAsync.DelayAsync(() => 1f / gun.FireRate, () => Services.GamePauser.Paused == true, token);
            }

            ClearOnShotFired();
        }

        protected override void OnMove(Rigidbody2D body, MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        }

        protected override void OnHit(object shooter, HitEventArgs e, float damageFactor = 1f)
        {
            e.Damageable.ApplyDamage(Damage);

            if (e.ObjectBeingHit.TryGetComponent(out INaniteTarget target) == true)
                target.TryApplyNanitesAsync(_nanites).Forget();
        }

        protected override void OnMiss(object shooter) { }

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