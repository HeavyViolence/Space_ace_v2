using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Cluster ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Cluster ammo set config")]
    public sealed class ClusterAmmoSetConfig : AmmoSetConfig
    {
        public const int MinProjectilesPerShot = 2;
        public const int MaxProjectilesPerShot = 20;

        [SerializeField, MinMaxSlider(MinProjectilesPerShot, MaxProjectilesPerShot), Space]
        private Vector2Int _projectilesPerShot = new(MinProjectilesPerShot, MaxProjectilesPerShot);

        public Vector2Int ProjectilesPerShot => _projectilesPerShot;

        public override AmmoType AmmoType => AmmoType.Cluster;
    }
}