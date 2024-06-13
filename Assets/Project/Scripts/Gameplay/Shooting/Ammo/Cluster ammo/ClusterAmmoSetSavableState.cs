using SpaceAce.Gameplay.Items;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class ClusterAmmoSetSavableState : AmmoSetSavableState
    {
        public int ProjectilesPerShot { get; }

        public ClusterAmmoSetSavableState(Size size,
                                          Quality quality,
                                          float price,
                                          int amount,
                                          float heatGeneration,
                                          float speed,
                                          float damage,
                                          int projectilesPerShot) : base(size,
                                                                         quality,
                                                                         price,
                                                                         amount,
                                                                         heatGeneration,
                                                                         speed,
                                                                         damage)
        {
            ProjectilesPerShot = projectilesPerShot;
        }
    }
}