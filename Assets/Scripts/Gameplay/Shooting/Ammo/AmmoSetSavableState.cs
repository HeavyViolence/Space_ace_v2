using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public abstract class AmmoSetSavableState : ItemSavableState
    {
        public AmmoType Type { get; }
        public int Amount { get; }
        public float HeatGeneration { get; }
        public float Speed { get; }
        public float Damage { get; }

        public AmmoSetSavableState(AmmoType type,
                                   Size size,
                                   Quality quality,
                                   float price,
                                   int amount,
                                   float heatGeneration,
                                   float speed,
                                   float damage) : base(size, quality, price)
        {
            Type = type;
            Amount = amount;
            HeatGeneration = heatGeneration;
            Speed = speed;
            Damage = damage;
        }
    }
}