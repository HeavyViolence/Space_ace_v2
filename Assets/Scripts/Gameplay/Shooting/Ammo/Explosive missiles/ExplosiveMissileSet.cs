using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class ExplosiveMissileSet : MissileSet, IEquatable<ExplosiveMissileSet>
    {
        private static readonly int s_damageMask = LayerMask.GetMask("Player", "Enemies", "Bosses", "Meteors", "Wrecks", "Bombs");

        public float ExplosionDamage { get; }
        public float ExplosionRadius { get; }

        protected override HitBehaviour HitBehaviour => delegate (object shooter, HitEventArgs hitArgs)
        {
            hitArgs.DamageReceiver.ApplyDamage(Damage);
            RaycastHit2D[] hits = Physics2D.CircleCastAll(hitArgs.HitPosition, ExplosionRadius, Vector2.zero, float.PositiveInfinity, s_damageMask);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject.TryGetComponent(out IDamageable damageReceiver) == true)
                {
                    float explosionDamage = Mathf.Lerp(ExplosionDamage, 0f, hit.distance / ExplosionRadius);
                    damageReceiver.ApplyDamage(explosionDamage);
                }
            }
        };

        public ExplosiveMissileSet(AmmoServices services,
                                   ExplosiveMissileSetConfig config,
                                   ExplosiveMissileSetSavableState savedState) : base(services, config, savedState)
        {
            ExplosionDamage = savedState.ExplosionDamage;
            ExplosionRadius = savedState.ExplosionRadius;
        }

        public ExplosiveMissileSet(AmmoServices services,
                                   Size size,
                                   Quality quality,
                                   ExplosiveMissileSetConfig config) : base(services, size, quality, config)
        {
            ExplosionDamage = services.ItemPropertyEvaluator.Evaluate(config.ExplosionDamage, true, quality, size, SizeInfluence.Direct);
            ExplosionRadius = services.ItemPropertyEvaluator.Evaluate(config.ExplosionRadius, true, quality, size, SizeInfluence.Direct);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Explosive missile/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Explosive missile/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Explosive missile/Code", this);

        public override ItemSavableState GetSavableState() => new ExplosiveMissileSetSavableState(Size,
                                                                                                  Quality,
                                                                                                  Price,
                                                                                                  Amount,
                                                                                                  HeatGeneration,
                                                                                                  Speed,
                                                                                                  Damage,
                                                                                                  HomingSpeed,
                                                                                                  TargetingWidth,
                                                                                                  SpeedGainDuration,
                                                                                                  ExplosionRadius,
                                                                                                  ExplosionDamage);

        #region interfaces

        public override bool Equals(MissileSet other) => base.Equals(other) && Equals(other as ExplosiveMissileSet);

        public bool Equals(ExplosiveMissileSet other) => other is not null &&
                                                         ExplosionDamage == other.ExplosionDamage &&
                                                         ExplosionRadius == other.ExplosionRadius;

        public override int GetHashCode() => base.GetHashCode() ^
                                             ExplosionDamage.GetHashCode() ^
                                             ExplosionRadius.GetHashCode();

        #endregion
    }
}