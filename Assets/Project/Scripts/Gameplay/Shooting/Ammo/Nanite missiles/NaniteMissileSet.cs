using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Items;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class NaniteMissileSet : MissileSet, IEquatable<NaniteMissileSet>
    {
        private readonly Nanites _nanites;

        public float ExplosionRadius { get; }
        public float DamagePerSecond => _nanites.DamagePerSecond;
        public float DamageDuration => _nanites.DamageDuration;

        protected override void OnHit(object shooter, HitEventArgs e)
        {
            e.Damageable.ApplyDamage(Damage);

            RaycastHit2D[] hits = Physics2D.CircleCastAll(e.HitPosition, ExplosionRadius, Vector2.zero);

            foreach (RaycastHit2D hit in hits)
                if (hit.collider.gameObject.TryGetComponent(out INaniteTarget target) == true)
                    target.TryApplyNanitesAsync(_nanites).Forget();
        }

        public NaniteMissileSet(AmmoServices services,
                                NaniteMissileSetConfig config,
                                NaniteMissileSetSavableState savedState) : base(services, config, savedState)
        {
            ExplosionRadius = savedState.ExplosionRadius;
            _nanites = new(savedState.DamagePerSecond, savedState.DamageDuration);
        }

        public NaniteMissileSet(AmmoServices services,
                                Size size,
                                Quality quality,
                                NaniteMissileSetConfig config) : base(services, size, quality, config)
        {
            ExplosionRadius = services.ItemPropertyEvaluator.Evaluate(config.ExplosionRadius, true, quality, size, SizeInfluence.Direct);

            float damagePerSecond = services.ItemPropertyEvaluator.Evaluate(config.DamagePerSecond, true, quality, size, SizeInfluence.Direct);
            float damageDuration = services.ItemPropertyEvaluator.Evaluate(config.DamageDuration, true, quality, size, SizeInfluence.Direct);

            _nanites = new(damagePerSecond, damageDuration);
        }

        public async override UniTask<string> GetDescriptionAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Nanite missile/Description", this);

        public async override UniTask<string> GetNameAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Nanite missile/Name", this);

        public async override UniTask<string> GetTypeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Nanite missile/Code", this);

        public override ItemSavableState GetSavableState() => new NaniteMissileSetSavableState(Size,
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
                                                                                               DamagePerSecond,
                                                                                               DamageDuration);

        #region interfaces

        public override bool Equals(MissileSet other) => base.Equals(other) && Equals(other as NaniteMissileSet);

        public bool Equals(NaniteMissileSet other) => other is not null &&
                                                      ExplosionRadius == other.ExplosionRadius &&
                                                      DamagePerSecond == other.DamagePerSecond &&
                                                      DamageDuration == other.DamageDuration;

        public override int GetHashCode() => base.GetHashCode() ^
                                             ExplosionRadius.GetHashCode() ^
                                             DamagePerSecond.GetHashCode() ^
                                             DamageDuration.GetHashCode();

        #endregion
    }
}