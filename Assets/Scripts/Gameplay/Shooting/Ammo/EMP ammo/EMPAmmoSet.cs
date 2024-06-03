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
    public sealed class EMPAmmoSet : AmmoSet, IEquatable<EMPAmmoSet>
    {
        private readonly EMP _emp;

        public float EMPStrength => _emp.Strength;
        public float EMPStrengthPercentage => _emp.StrengthPercentage;

        public float EMPDuration => _emp.Duration;

        public override ShotBehaviour ShotBehaviour => delegate (object shooter, IGunView gunView)
        {
            ProjectileCache projectile = Services.ProjectileFactory.Create(shooter, ProjectileSkin, Size);

            float dispersion = AuxMath.RandomUnit * gunView.Dispersion;

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

            Services.AudioPlayer.PlayOnceAsync(ShotAudio.Random, gunView.Transform.position, null, true).Forget();

            if (shooter is Player)
            {
                Amount--;
                Price -= ShotPrice;
            }

            return new(1, HeatGeneration);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        protected override HitBehaviour HitBehaviour => delegate (object shooter, HitEventArgs hitArgs)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);

            foreach (var target in hitArgs.ObjectBeingHit.GetComponentsInChildren<IEMPTarget>())
                target.TryApplyEMPAsync(_emp).Forget();
        };

        protected override MissBehaviour MissBehaviour => null;

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