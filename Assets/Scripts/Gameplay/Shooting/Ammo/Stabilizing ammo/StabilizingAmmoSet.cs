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

        public override ShotBehaviour ShotBehaviour => delegate (object shooter, IGunView gunView)
        {
            ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

            if (gunView.FirstShotInLine == true) _currentDispersionFactor = 1f;
            else _currentDispersionFactor *= DispersionFactorPerShot;

            float dispersion = AuxMath.RandomUnit * gunView.Dispersion * _currentDispersionFactor;

            Vector2 projectileDirection = new(gunView.Transform.up.x + gunView.SignedConvergenceAngle + dispersion, gunView.Transform.up.y);
            projectileDirection.Normalize();

            Quaternion projectileRotation = gunView.Transform.rotation * Quaternion.Euler(0f, 0f, gunView.SignedConvergenceAngle + dispersion);
            projectileRotation.Normalize();

            MovementData data = new(Speed, Speed, 0f, gunView.Transform.position, projectileDirection, projectileRotation, null, 0f, 0f);

            projectile.Transform.SetPositionAndRotation(gunView.Transform.position, projectileRotation);
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

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gunView.Transform.position, null, true).Forget();

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