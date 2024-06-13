using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class DevastatingAmmoSetSavableState : AmmoSetSavableState
    {
        public float ConsecutiveDamageFactor { get; }

        public DevastatingAmmoSetSavableState(Size size,
                                              Quality quality,
                                              float price,
                                              int amount,
                                              float heatGeneration,
                                              float speed,
                                              float damage,
                                              float consecutiveDamageFactor) : base(size,
                                                                                    quality,
                                                                                    price,
                                                                                    amount,
                                                                                    heatGeneration,
                                                                                    speed,
                                                                                    damage)
        { }
    }
}