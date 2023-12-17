using Cysharp.Threading.Tasks;

using SpaceAce.Main;
using SpaceAce.Main.Localization;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Inventories
{
    public abstract class Item : IEquatable<Item>
    {
        public float Grade { get; }
        public float Price { get; }
        public float Duration { get; }

        public bool Usable { get; }
        public bool Tradable { get; }

        protected Localizer Localizer { get; }
        protected GameStateLoader GameStateLoader { get; }

        public Item(float grade,
                    float price,
                    float duration,
                    bool usable,
                    bool tradable,
                    Localizer localizer,
                    GameStateLoader loader)
        {
            Grade = Mathf.Clamp(grade, 0f, float.PositiveInfinity);
            Price = Mathf.Clamp(price, 0f, float.PositiveInfinity);
            Duration = Mathf.Clamp(duration, 0f, float.PositiveInfinity);

            Usable = usable;
            Tradable = tradable;

            Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer),
                $"Attempted to pass an empty {typeof(Localizer)}!");

            GameStateLoader = loader ?? throw new ArgumentNullException(nameof(loader),
                $"Attempted to pass an empty {typeof(GameStateLoader)}!");
        }

        public abstract bool Use();
        public abstract bool Sell(out float sellPrice);

        public abstract UniTask<string> GetNameAsync();
        public abstract UniTask<string> GetDescriptionAsync();
        public abstract UniTask<string> GetStatsAsync();
        public abstract UniTask<Sprite> GetIconAsync();

        public abstract ItemSnapshot GetSnapshot();

        #region interfaces

        public override bool Equals(object obj) => Equals(obj as Item);

        public virtual bool Equals(Item other) => other is not null &&
                                                           Grade == other.Grade &&
                                                           Price == other.Price &&
                                                           Duration == other.Duration &&
                                                           Usable == other.Usable &&
                                                           Tradable == other.Tradable;

        public override int GetHashCode() => Grade.GetHashCode() ^
                                             Price.GetHashCode() ^
                                             Duration.GetHashCode() ^
                                             Usable.GetHashCode() ^
                                             Tradable.GetHashCode();

        #endregion
    }
}