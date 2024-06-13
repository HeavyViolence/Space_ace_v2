using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public class MissileSetSavableState : HomingAmmoSetSavableState
    {
        public float SpeedGainDuration { get; }

        public MissileSetSavableState(Size size,
                                      Quality quality,
                                      float price,
                                      int amount,
                                      float heatGeneration,
                                      float speed,
                                      float damage,
                                      float homingSpeed,
                                      float targetingWidth,
                                      float speedGainDuration) : base(size,
                                                                      quality,
                                                                      price,
                                                                      amount,
                                                                      heatGeneration,
                                                                      speed,
                                                                      damage,
                                                                      homingSpeed,
                                                                      targetingWidth)
        {
            SpeedGainDuration = speedGainDuration;
        }
    }
}