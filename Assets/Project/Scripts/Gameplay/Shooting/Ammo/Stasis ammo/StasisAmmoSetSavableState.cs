using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class StasisAmmoSetSavableState : AmmoSetSavableState
    {
        public float Slowdown { get; }
        public float Duration { get; }

        public StasisAmmoSetSavableState(Size size,
                                         Quality quality,
                                         float price,
                                         int amount,
                                         float heatGeneration,
                                         float speed,
                                         float damage,
                                         float slowdown,
                                         float duration) : base(size,
                                                                quality,
                                                                price,
                                                                amount,
                                                                heatGeneration,
                                                                speed,
                                                                damage)
        {
            Slowdown = slowdown;
            Duration = duration;
        }
    }
}