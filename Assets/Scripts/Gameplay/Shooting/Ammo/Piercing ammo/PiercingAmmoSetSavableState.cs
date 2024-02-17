using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class PiercingAmmoSetSavableState : AmmoSetSavableState
    {
        public int ProjectileHits { get; }
        public float HeatGenerationFactorPerShot { get; }

        public PiercingAmmoSetSavableState(Size size, 
                                           Quality quality, 
                                           float price, 
                                           int amount, 
                                           float heatGeneration, 
                                           float speed, 
                                           float damage,
                                           int projectileHits,
                                           float heatGenerationFactorPerShot) : base(size, 
                                                                                     quality, 
                                                                                     price, 
                                                                                     amount, 
                                                                                     heatGeneration, 
                                                                                     speed, 
                                                                                     damage)
        {
            ProjectileHits = projectileHits;
            HeatGenerationFactorPerShot = heatGenerationFactorPerShot;
        }
    }
}