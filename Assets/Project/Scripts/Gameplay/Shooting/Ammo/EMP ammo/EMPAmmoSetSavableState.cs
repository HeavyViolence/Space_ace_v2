using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class EMPAmmoSetSavableState : AmmoSetSavableState
    {
        public float EMPStrength { get; }
        public float EMPDuration { get; }

        public EMPAmmoSetSavableState(Size size,
                                      Quality quality,
                                      float price,
                                      int amount,
                                      float heatGeneration,
                                      float speed,
                                      float damage,
                                      float empStrength,
                                      float empDuration) : base(size,
                                                                quality,
                                                                price,
                                                                amount,
                                                                heatGeneration,
                                                                speed,
                                                                damage)
        {
            EMPStrength = empStrength;
            EMPDuration = empDuration;
        }
    }
}