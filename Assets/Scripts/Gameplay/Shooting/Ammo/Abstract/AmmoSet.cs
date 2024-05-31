using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting.Guns;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories.ProjectileFactories;
using SpaceAce.Main.Factories.ProjectileHitEffectFactories;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public abstract class AmmoSet : IItem, IEquatable<AmmoSet>
    {
        public event EventHandler<IntValueChangedEventArgs> AmountChanged;
        public event EventHandler Depleted;

        protected readonly AmmoServices Services;

        public Sprite Icon => Services.ItemIconProvider.GetItemIcon(this);
        public AudioCollection ShotAudio { get; }

        public Size Size { get; }
        public Quality Quality { get; }
        public ProjectileSkin ProjectileSkin { get; }
        public ProjectileHitEffectSkin HitEffectSkin { get; }

        private int _amount;
        public int Amount
        {
            get => _amount;

            protected set
            {
                int oldAmount = _amount;

                _amount = Mathf.Clamp(value, 0, int.MaxValue);

                AmountChanged?.Invoke(this, new(oldAmount, _amount));

                if (_amount == 0) Depleted?.Invoke(this, EventArgs.Empty);
            }
        }

        public float Price { get; protected set; }
        public float ShotPrice { get; }
        public float HeatGeneration { get; }
        public float Speed { get; }
        public float Damage { get; }

        public bool Usable => false;
        public bool Tradable => Services.GameStateLoader.CurrentState == GameState.MainMenu;

        protected abstract ShotBehaviourAsync ShotBehaviourAsync { get; }
        protected abstract MovementBehaviour MovementBehaviour { get; }
        protected abstract HitBehaviour HitBehaviour { get; }
        protected abstract MissBehaviour MissBehaviour { get; }

        public AmmoSet(AmmoServices services, Size size, Quality quality, AmmoSetConfig config)
        {
            if (config == null) throw new ArgumentNullException();

            Services = services;
            Size = size;
            Quality = quality;
            ProjectileSkin = config.ProjectileSkin;
            HitEffectSkin = config.HitEffectSkin;
            ShotAudio = config.ShotAudio;

            Amount = services.ItemPropertyEvaluator.Evaluate(config.Amount, true, quality, size, SizeInfluence.None);
            Price = services.ItemPropertyEvaluator.EvaluatePrice(config.Price, quality, size);
            ShotPrice = Price / Amount;
            HeatGeneration = services.ItemPropertyEvaluator.Evaluate(config.HeatGeneration, false, quality, size, SizeInfluence.Direct);
            Speed = services.ItemPropertyEvaluator.Evaluate(config.Speed, true, quality, size, SizeInfluence.Inverted);
            Damage = services.ItemPropertyEvaluator.Evaluate(config.Damage, true, quality, size, SizeInfluence.Direct);
        }

        public AmmoSet(AmmoServices services, AmmoSetConfig config, AmmoSetSavableState savedState)
        {
            if (config == null) throw new ArgumentNullException();
            if (savedState == null) throw new ArgumentNullException();

            Services = services;
            Size = savedState.Size;
            Quality = savedState.Quality;
            ProjectileSkin = config.ProjectileSkin;
            HitEffectSkin = config.HitEffectSkin;
            ShotAudio = config.ShotAudio;
            Amount = savedState.Amount;
            Price = savedState.Price;
            ShotPrice = Amount / Price;
            HeatGeneration = savedState.HeatGeneration;
            Speed = savedState.Speed;
            Damage = savedState.Damage;
        }

        ~AmmoSet()
        {
            AmountChanged = null;
            Depleted = null;
        }

        public async UniTask<bool> TryUseAsync(object user, CancellationToken token = default)
        {
            await UniTask.Yield();
            return false;
        }

        public async UniTask<ShotResult> FireAsync(object shooter, IGun gun)
        {
            if (shooter is null) throw new ArgumentNullException();
            if (gun is null) throw new ArgumentNullException();

            if (Services.GameStateLoader.CurrentState == GameState.Level)
                return await ShotBehaviourAsync.Invoke(shooter, gun);

            return ShotResult.None;
        }

        public abstract ItemSavableState GetSavableState();

        public abstract UniTask<string> GetNameAsync();
        public abstract UniTask<string> GetTypeCodeAsync();
        public abstract UniTask<string> GetDescriptionAsync();

        #region interfaces

        public override bool Equals(object obj) => obj is not null && Equals(obj as IItem);

        public bool Equals(IItem other) => other is not null && Equals(other as AmmoSet);

        public virtual bool Equals(AmmoSet ammo) => ammo is not null &&
                                                    Size == ammo.Size &&
                                                    Quality == ammo.Quality &&
                                                    ProjectileSkin == ammo.ProjectileSkin &&
                                                    HitEffectSkin == ammo.HitEffectSkin &&
                                                    ShotAudio == ammo.ShotAudio &&
                                                    Price == ammo.Price &&
                                                    HeatGeneration == ammo.HeatGeneration &&
                                                    Speed == ammo.Speed &&
                                                    Damage == ammo.Damage &&
                                                    Amount == ammo.Amount;

        public override int GetHashCode() => Size.GetHashCode() ^
                                             Quality.GetHashCode() ^
                                             ProjectileSkin.GetHashCode() ^
                                             HitEffectSkin.GetHashCode() ^
                                             ShotAudio.GetHashCode() ^
                                             Price.GetHashCode() ^
                                             HeatGeneration.GetHashCode() ^
                                             Speed.GetHashCode() ^
                                             Damage.GetHashCode() ^
                                             Amount.GetHashCode();

        #endregion
    }
}