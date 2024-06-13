using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main.Factories.ProjectileFactories;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class EMPAmmoSet : AmmoSet, IEquatable<EMPAmmoSet>
    {
        private readonly EMP _emp;

        public float EMPStrength => _emp.Strength;
        public float EMPStrengthPercentage => _emp.StrengthPercentage;

        public float EMPDuration => _emp.Duration;

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

                Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();
                if (gun.ShakeOnShotFired == true) Services.MasterCameraShaker.ShakeOnShotFired();

                OnShotFired(HeatGeneration);

                await UniTask.WaitUntil(() => Services.GamePauser.Paused == false);
                await UniTask.WaitForSeconds(1f / gun.FireRate);
            }

            ClearOnShotFired();
        }

        protected override void OnMove(Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        }

        protected override void OnHit(object shooter, HitEventArgs e)
        {
            e.Damageable.ApplyDamage(Damage);

            if (e.ObjectBeingHit.TryGetComponent(out IEMPTargetProvider provider) == true)
                foreach (var target in provider.GetTargets())
                    target.TryApplyEMPAsync(_emp).Forget();
        }

        protected override void OnMiss(object shooter) { }

        public EMPAmmoSet(AmmoServices services,
                          EMPAmmoSetConfig config,
                          EMPAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            _emp = new(savedState.EMPStrength, savedState.EMPDuration, config.EMPStrenghtOverTime);
        }

        public EMPAmmoSet(AmmoServices services,
                          Size size,
                          Quality quality,
                          EMPAmmoSetConfig config) : base(services, size, quality, config)
        {
            float empStrength = services.ItemPropertyEvaluator.Evaluate(config.EMPStrength, true, quality, size, SizeInfluence.None);
            float empDuration = services.ItemPropertyEvaluator.Evaluate(config.EMPDuration, true, quality, size, SizeInfluence.Direct);

            _emp = new(empStrength, empDuration, config.EMPStrenghtOverTime);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "EMP/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "EMP/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "EMP/Code", this);

        public override ItemSavableState GetSavableState() =>
            new EMPAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, EMPStrength, EMPDuration);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as EMPAmmoSet);

        public bool Equals(EMPAmmoSet other) => other is not null &&
                                                EMPStrength == other.EMPStrength &&
                                                EMPDuration == other.EMPDuration;

        public override int GetHashCode() => base.GetHashCode() ^
                                             EMPStrength.GetHashCode() ^
                                             EMPDuration.GetHashCode();

        #endregion
    }
}