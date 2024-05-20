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
    public sealed class CoolingAmmoSet : AmmoSet, IEquatable<CoolingAmmoSet>
    {
        private float _currentHeatGenerationFactor = 1f;

        public float HeatGenerationFactorPerShot { get; }
        public float HeatGenerationDecreasePerShotPercentage => (1f - HeatGenerationFactorPerShot) * 100f;

        protected override ShotBehaviourAsync ShotBehaviourAsync => async delegate (object shooter, IGun gun)
        {
            ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

            float dispersion = AuxMath.RandomUnit * gun.Dispersion;

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

            if (shooter is IAmmoObservable observable)
            {
                if (observable.Shooter.FirstShotInLine == true) _currentHeatGenerationFactor = 1f;
                else _currentHeatGenerationFactor *= HeatGenerationFactorPerShot;
            }
            else
            {
                throw new Exception($"{nameof(CoolingAmmoSet)} user doesn't implement {typeof(IAmmoObservable)}!");
            }

            await UniTask.WaitForSeconds(1f / gun.FireRate);

            return new(1, HeatGeneration * _currentHeatGenerationFactor);
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

        public CoolingAmmoSet(AmmoServices services,
                              CoolingAmmoSetConfig config,
                              CoolingAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            HeatGenerationFactorPerShot = savedState.HeatGenerationFactorPerShot;
        }

        public CoolingAmmoSet(AmmoServices services,
                              Size size,
                              Quality quality,
                              CoolingAmmoSetConfig config) : base(services, size, quality, config)
        {
            HeatGenerationFactorPerShot = services.ItemPropertyEvaluator.Evaluate(config.HeatGenerationFactorPerShot, false, quality, size, SizeInfluence.None);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Cooling/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Cooling/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Cooling/Code", this);

        public override ItemSavableState GetSavableState() =>
            new CoolingAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, HeatGenerationFactorPerShot);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as CoolingAmmoSet);

        public bool Equals(CoolingAmmoSet other) => other is not null &&
                                                    HeatGenerationFactorPerShot == other.HeatGenerationFactorPerShot;

        public override int GetHashCode() => base.GetHashCode() ^ HeatGenerationFactorPerShot.GetHashCode();

        #endregion
    }
}