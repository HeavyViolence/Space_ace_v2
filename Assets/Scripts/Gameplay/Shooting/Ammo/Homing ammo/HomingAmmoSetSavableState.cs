using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public class HomingAmmoSetSavableState : AmmoSetSavableState
    {
        public float HomingSpeed { get; }
        public float TargetingWidth { get; }

        public HomingAmmoSetSavableState(Size size,
                                         Quality quality,
                                         float price,
                                         int amount,
                                         float heatGeneration,
                                         float speed,
                                         float damage,
                                         float homingSpeed,
                                         float targetingWidth) : base(size,
                                                                      quality,
                                                                      price,
                                                                      amount,
                                                                      heatGeneration,
                                                                      speed,
                                                                      damage)
        {
            HomingSpeed = homingSpeed;
            TargetingWidth = targetingWidth;
        }
    }
}