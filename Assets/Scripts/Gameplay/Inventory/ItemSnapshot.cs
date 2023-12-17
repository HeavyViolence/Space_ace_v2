using SpaceAce.Main;
using SpaceAce.Main.Localization;

namespace SpaceAce.Gameplay.Inventories
{
    public abstract class ItemSnapshot
    {
        public float Grade { get; }
        public float Price { get; }
        public float Duration { get; }

        public bool Usable { get; }
        public bool Tradable { get; }

        public ItemSnapshot(float grade,
                            float price,
                            float duration,
                            bool usable,
                            bool tradable)
        {
            Grade = grade;
            Price = price;
            Duration = duration;
            Usable = usable;
            Tradable = tradable;
        }

        public abstract Item RecreateItem(Localizer localizer, GameStateLoader loader);
    }
}