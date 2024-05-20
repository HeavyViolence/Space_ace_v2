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
    public sealed class StabilizingAmmoSet : AmmoSet, IEquatable<StabilizingAmmoSet>
    {
        private float _currentDispersionFactor = 1f;

        public float DispersionFactorPerShot { get; }
        public float DispersionDecreasePerShotPercentage => (1f - DispersionFactorPerShot) * 100f;

        protected override ShotBehaviourAsync ShotBehaviourAsync => async delegate (object shooter, IGun gun)
        {
            ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

            if (shooter is IAmmoObservable observable)
            {
                if (observable.Shooter.FirstShotInLine == true) _currentDispersionFactor = 1f;
                else _currentDispersionFactor *= DispersionFactorPerShot;
            }
            else
            {
                throw new MissingComponentException($"{typeof(IAmmoObservable)}");
            }

            float dispersion = AuxMath.RandomUnit * gun.Dispersion * _currentDispersionFactor;

            Vector2 projectileDirection = new(gun.Transform.up.x + gun.SignedConvergenceAngle + dispersion, gun.Transform.up.y);
            projectileDirection.Normalize();

            Quaternion projectileRotation = gun.Transform.rotation * Quaternion.Euler(0f, 0f, gun.SignedConvergenceAngle + dispersion);
            projectileRotation.Normalize();

            MovementData data = new(Speed, Speed, 0f, gun.Transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

            projectile.Object.transform.SetPositionAndRotation(gun.Transform.position, projectileRotation);
            projectile.MovementBehaviourSupplier.Supply(MovementBehaviour, data);

            projectile.DamageDealer.Hit += (sender, hitArgs) =>
            {
                HitBehaviour?.Invoke(shooter, hitArgs);
                Services.ProjectileFactory.Release(ProjectileSkin, projectile);
                Services.ProjectileHitEffectFactory.CreateAsync(HitEffectSkin, hitArgs.HitPosition).Forget();
            };

            projectile.Escapable.Escaped += (sender, args) =>
            {
                MissBehaviour?.Invoke(shooter);
                Services.ProjectileFactory.Release(ProjectileSkin, projectile);
            };

            if (shooter is Player)
            {
                Amount--;
                Price -= ShotPrice;
            }

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gun.Transform.position, null, true).Forget();

            await UniTask.WaitForSeconds(1f / gun.FireRate);

            return new(1, HeatGeneration);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        protected override HitBehaviour HitBehaviour => delegate (object shooter, HitEventArgs hitArgs)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);
        };

        protected override MissBehaviour MissBehaviour => null;

        public StabilizingAmmoSet(AmmoServices services,
                                  StabilizingAmmoSetConfig config,
                                  StabilizingAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            DispersionFactorPerShot = savedState.DispersionFactorPerShot;
        }

        public StabilizingAmmoSet(AmmoServices services,
                                  Size size,
                                  Quality quality,
                                  StabilizingAmmoSetConfig config) : base(services, size, quality, config)
        {
            DispersionFactorPerShot = services.ItemPropertyEvaluator.Evaluate(config.DispersionFactorPerShot, false, quality, size, SizeInfluence.None);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Stabilizing/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Stabilizing/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Stabilizing/Code", this);

        public override ItemSavableState GetSavableState() =>
            new StabilizingAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, DispersionFactorPerShot);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as StabilizingAmmoSet);

        public bool Equals(StabilizingAmmoSet other) => other is not null &&
                                                        DispersionFactorPerShot == other.DispersionFactorPerShot;

        public override int GetHashCode() => base.GetHashCode() ^ DispersionFactorPerShot.GetHashCode();

        #endregion
    }
}