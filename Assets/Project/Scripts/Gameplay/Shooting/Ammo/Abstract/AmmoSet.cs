using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Effects;
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
    public abstract class AmmoSet : IItem, IEquatable<AmmoSet>, IDisposable, IEMPTarget
    {
        public event EventHandler<ShotFiredEventArgs> ShotFired;
        public event EventHandler<IntValueChangedEventArgs> AmountChanged;
        public event EventHandler Depleted;

        protected static readonly int PlayerTargetsMask = LayerMask.GetMask("Enemies", "Bosses", "Meteors", "Wrecks", "Bombs");
        protected static readonly int EnemyTargetsMask = LayerMask.GetMask("Player");
        protected static readonly int AllTargetsMask = LayerMask.GetMask("Player", "Enemies", "Bosses", "Meteors", "Wrecks", "Bombs");

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
                int newAmount = Mathf.Clamp(value, 0, AmmoSetConfig.MaxAmount);
                int delta = newAmount - oldAmount;

                _amount = newAmount;
                Price += delta < 0 ? -1f * ShotPrice * delta : ShotPrice * delta;

                if (_amount == 0)
                {
                    Depleted?.Invoke(this, EventArgs.Empty);
                    Price = 0f;
                }

                AmountChanged?.Invoke(this, new(oldAmount, newAmount));
            }
        }

        public float Price { get; private set; }
        public float ShotPrice { get; }
        public float HeatGeneration { get; }
        public float Speed { get; }
        public float Damage { get; }

        public bool Usable => false;
        public bool Tradable => Services.GameStateLoader.CurrentState == GameState.MainMenu;

        public abstract UniTask FireAsync(object shooter,
                                          IGun gun,
                                          CancellationToken fireCancellation = default,
                                          CancellationToken overheatCancellation = default);
        protected abstract void OnMove(Rigidbody2D body, MovementData data);
        protected abstract void OnHit(object shooter, HitEventArgs e, float damageFactor = 1f);
        protected abstract void OnMiss(object shooter);

        public AmmoSet(AmmoServices services, Size size, Quality quality, AmmoSetConfig config)
        {
            if (config == null) throw new ArgumentNullException();

            Services = services;
            Size = size;
            Quality = quality;
            ProjectileSkin = config.ProjectileSkin;
            HitEffectSkin = config.HitEffectSkin;
            ShotAudio = config.ShotAudio;

            Amount = services.ItemPropertyEvaluator.Evaluate(config.Amount, RangeEvaluationDirection.Forward, quality, size, SizeInfluence.None);
            Price = services.ItemPropertyEvaluator.EvaluatePrice(config.Price, quality, size);
            ShotPrice = Price / Amount;
            HeatGeneration = services.ItemPropertyEvaluator.Evaluate(config.HeatGeneration, RangeEvaluationDirection.Forward, quality, size, SizeInfluence.Inverted);
            Speed = services.ItemPropertyEvaluator.Evaluate(config.Speed, RangeEvaluationDirection.Forward, quality, size, SizeInfluence.Inverted);
            Damage = services.ItemPropertyEvaluator.Evaluate(config.Damage, RangeEvaluationDirection.Forward, quality, size, SizeInfluence.Direct);
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

        public async UniTask<bool> TryUseAsync(object user, CancellationToken token = default)
        {
            await UniTask.Yield();
            return false;
        }

        public abstract ItemSavableState GetSavableState();

        public abstract UniTask<string> GetNameAsync();
        public abstract UniTask<string> GetTypeCodeAsync();
        public abstract UniTask<string> GetDescriptionAsync();

        public async UniTask<string> GetSizeCodeAsync() =>
            await Services.Localizer.GetLocalizedStringAsync("Ammo", "Size code", this);

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
                                                    Speed == ammo.Speed &&
                                                    Damage == ammo.Damage &&
                                                    Amount == ammo.Amount &&
                                                    HeatGeneration == ammo.HeatGeneration;

        public override int GetHashCode() => Size.GetHashCode() ^
                                             Quality.GetHashCode() ^
                                             ProjectileSkin.GetHashCode() ^
                                             HitEffectSkin.GetHashCode() ^
                                             ShotAudio.GetHashCode() ^
                                             Price.GetHashCode() ^
                                             Speed.GetHashCode() ^
                                             Damage.GetHashCode() ^
                                             Amount.GetHashCode() ^
                                             HeatGeneration.GetHashCode();

        public void Dispose()
        {
            ShotFired = null;
            AmountChanged = null;
            Depleted = null;
        }

        #endregion

        protected void OnShotFired(float heat) => ShotFired?.Invoke(this, new(heat));
        protected void ClearOnShotFired() => ShotFired = null;

        #region EMP target interface

        public bool EMPActive { get; private set; } = false;
        public float EMPFactor { get; private set; } = 0f;

        public async UniTask<bool> TryApplyEMPAsync(EMP emp, CancellationToken token = default)
        {
            if (EMPActive == true) return false;

            EMPActive = true;
            float timer = 0f;

            while (timer < emp.Duration)
            {
                if (token.IsCancellationRequested == true) break;

                timer += Time.fixedDeltaTime;

                EMPFactor = emp.GetFactor(timer);

                await UniTask.WaitUntil(() => Services.GamePauser.Paused == false);
                await UniTask.WaitForFixedUpdate();
            }

            EMPFactor = 0f;
            EMPActive = false;

            return true;
        }

        #endregion
    }
}