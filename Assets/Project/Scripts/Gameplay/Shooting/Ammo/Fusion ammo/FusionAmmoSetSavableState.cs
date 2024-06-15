using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class FusionAmmoSetSavableState : AmmoSetSavableState
    {
        public float ArmorIgnoring { get; }

        public FusionAmmoSetSavableState(Size size,
                                         Quality quality,
                                         float price,
                                         int amount,
                                         float heatGeneration,
                                         float speed,
                                         float damage,
                                         float armorIgnoring) : base(size,
                                                                     quality,
                                                                     price,
                                                                     amount,
                                                                     heatGeneration,
                                                                     speed,
                                                                     damage)
        {
            ArmorIgnoring = armorIgnoring;
        }
    }
}