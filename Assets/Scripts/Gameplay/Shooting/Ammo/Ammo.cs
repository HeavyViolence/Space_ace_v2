using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Inventories;
using SpaceAce.Gameplay.Shooting.Guns;
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
        public ProjectileSkin Skin { get; }

        public float Price { get; }
        public float HeatGeneration { get; }
        public float Speed { get; }
        public float Damage { get; }

        public bool Usable => false;
        public bool Tradable => Services.GameStateLoader.CurrentState == GameState.MainMenu;

        protected abstract Func<Gun, CancellationToken, UniTask> ShotBehaviour { get; }

        public Ammo(AmmoServices services,
                    ItemSize size,
                    ItemQuality quality,
                    ProjectileSkin skin,
                    AmmoConfig config)
        {
            Services = services;

            Size = size;
            Quality = quality;
            Skin = skin;

            if (config == null) throw new ArgumentNullException();

            Price = config.GetPrice(size, quality);
            HeatGeneration = config.GetHeatGeneration(size, quality);
            Speed = config.GetSpeed(size, quality);
            Damage = config.GetDamage(size, quality);
            ShotAudio = config.ShotAudio;
        }

        public bool Use() => false;

        public async UniTask ShootAsync(Gun gun, CancellationToken token) =>
            await ShotBehaviour.Invoke(gun, token);

        public abstract UniTask<string> GetNameAsync();
        public abstract UniTask<string> GetStatsAsync();

        protected void RaiseShotEvent(int ammoUsed, float heatGenerated) =>
            Shot?.Invoke(this, new(ammoUsed, heatGenerated));

        #region interfaces

        public override bool Equals(object obj) => obj is not null && Equals(obj as IItem);

        public bool Equals(IItem other) => other is not null && Equals(other as Ammo);

        public virtual bool Equals(Ammo ammo) => ammo is not null &&
                                                 Type == ammo.Type &&
                                                 Size == ammo.Size &&
                                                 Quality == ammo.Quality &&
                                                 Skin == ammo.Skin &&
                                                 Price == ammo.Price &&
                                                 HeatGeneration == ammo.HeatGeneration &&
                                                 Speed == ammo.Speed &&
                                                 Damage == ammo.Damage;

        public override int GetHashCode() => Type.GetHashCode() ^
                                             Size.GetHashCode() ^
                                             Quality.GetHashCode() ^
                                             Skin.GetHashCode() ^
                                             Price.GetHashCode() ^
                                             HeatGeneration.GetHashCode() ^
                                             Speed.GetHashCode() ^
                                             Damage.GetHashCode();

        #endregion
    }
}