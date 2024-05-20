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
    public sealed class DevastatingAmmoSet : AmmoSet, IEquatable<DevastatingAmmoSet>
    {
        private Guid _previousDamageReceiverID = Guid.Empty;
        private float _currentDamageFactor = 1f;

        public float ConsecutiveDamageFactor { get; }
        public float DamageIncreasePerHitPercentage => (ConsecutiveDamageFactor - 1f) * 100f;

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

            await UniTask.WaitForSeconds(1f / gun.FireRate);

            return new(1, HeatGeneration);
        };

        protected override MovementBehaviour MovementBehaviour => delegate (Rigidbody2D body, ref MovementData data)
        {
            body.MovePosition(body.position + data.InitialVelocityPerFixedUpdate);
        };

        protected override HitBehaviour HitBehaviour => delegate (object shooter, HitEventArgs hitArgs)
        {
            if (hitArgs.DamageReceiver.ID == _previousDamageReceiverID)
            {
                _currentDamageFactor *= ConsecutiveDamageFactor;
            }
            else
            {
                _previousDamageReceiverID = hitArgs.DamageReceiver.ID;
                _currentDamageFactor = 1f;
            }

            hitArgs.DamageReceiver.ApplyDamage(Damage * _currentDamageFactor);
        };

        protected override MissBehaviour MissBehaviour => null;

        public DevastatingAmmoSet(AmmoServices services,
                                  DevastatingAmmoSetConfig config,
                                  DevastatingAmmoSetSavableState savedState) : base(services, config, savedState)
        {
            ConsecutiveDamageFactor = savedState.ConsecutiveDamageFactor;
        }

        public DevastatingAmmoSet(AmmoServices services,
                                  Size size,
                                  Quality quality,
                                  DevastatingAmmoSetConfig config) : base(services, size, quality, config)
        {
            ConsecutiveDamageFactor = services.ItemPropertyEvaluator.Evaluate(config.ConsecutiveDamegeFactor, true, quality, size, SizeInfluence.None);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Devastating/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Devastating/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Devastating/Code", this);

        public override ItemSavableState GetSavableState() =>
            new DevastatingAmmoSetSavableState(Size, Quality, Price, Amount, HeatGeneration, Speed, Damage, ConsecutiveDamageFactor);

        #region interfaces

        public override bool Equals(AmmoSet ammo) => base.Equals(ammo) && Equals(ammo as DevastatingAmmoSet);

        public bool Equals(DevastatingAmmoSet other) => other is not null &&
                                                        ConsecutiveDamageFactor == other.ConsecutiveDamageFactor;

        public override int GetHashCode() => base.GetHashCode() ^ ConsecutiveDamageFactor.GetHashCode();

        #endregion
    }
}
