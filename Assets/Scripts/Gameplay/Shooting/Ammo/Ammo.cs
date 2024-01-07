using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Inventories;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories;

using System;
using System.Threading;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public abstract class Ammo : IItem, IEquatable<Ammo>
    {
        protected const float ProjectileReleaseDelay = 1f;

        public event EventHandler<ShotEventArgs> Shot;

        protected readonly AmmoServices Services;

        public abstract AmmoType Type { get; }

        public AudioCollection ShotAudio { get; }

        public ItemSize Size { get; }
        public ItemQuality Quality { get; }
        public ProjectileSkin ProjectileSkin { get; }
        public ProjectileHitEffectSkin HitEffectSkin { get; }

        public float Price { get; }
        public float HeatGeneration { get; }
        public float Speed { get; }
        public float Damage { get; }

        public bool Usable => Services.GameStateLoader.CurrentState == GameState.Level;
        public bool Tradable => Services.GameStateLoader.CurrentState == GameState.MainMenu;

        protected abstract ShootingBehaviour ShootingBehaviour { get; }
        protected abstract MovementBehaviour MovementBehaviour { get; }
        protected abstract HitBehaviour HitBehaviour { get; }
        protected abstract MissBehaviour MissBehaviour { get; }

        public Ammo(AmmoServices services,
                    ItemSize size,
                    ItemQuality quality,
                    ProjectileSkin projectileSkin,
                    ProjectileHitEffectSkin hitEffectSkin,
                    AmmoConfig config)
        {
            Services = services;

            Size = size;
            Quality = quality;
            ProjectileSkin = projectileSkin;
            HitEffectSkin = hitEffectSkin;

            if (config == null) throw new ArgumentNullException();

            Price = config.GetPrice(size, quality);
            HeatGeneration = config.GetHeatGeneration(size, quality);
            Speed = config.GetSpeed(size, quality);
            Damage = config.GetDamage(size, quality);
            ShotAudio = config.ShotAudio;
        }
        
        public async UniTask<bool> UseAsync(ItemStack holder,
                                            CancellationToken token = default,
                                            params object[] args)
        {
            if (Usable == true)
            {
                await ShootingBehaviour.Invoke(token, holder, args);
                return true;
            }

            return false;
        }

        public abstract UniTask<string> GetNameAsync();
        public abstract UniTask<string> GetDescriptionAsync();

        protected void RaiseShotEvent(float heatGenerated) => Shot?.Invoke(this, new(heatGenerated));

        #region interfaces

        public override bool Equals(object obj) => obj is not null && Equals(obj as IItem);

        public bool Equals(IItem other) => other is not null && Equals(other as Ammo);

        public virtual bool Equals(Ammo ammo) => ammo is not null &&
                                                 Type == ammo.Type &&
                                                 Size == ammo.Size &&
                                                 Quality == ammo.Quality &&
                                                 ProjectileSkin == ammo.ProjectileSkin &&
                                                 Price == ammo.Price &&
                                                 HeatGeneration == ammo.HeatGeneration &&
                                                 Speed == ammo.Speed &&
                                                 Damage == ammo.Damage;

        public override int GetHashCode() => Type.GetHashCode() ^
                                             Size.GetHashCode() ^
                                             Quality.GetHashCode() ^
                                             ProjectileSkin.GetHashCode() ^
                                             Price.GetHashCode() ^
                                             HeatGeneration.GetHashCode() ^
                                             Speed.GetHashCode() ^
                                             Damage.GetHashCode();

        #endregion
    }
}