using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Regular ammo set config",
                     menuName = "Space ace/Configs/Shooting/Ammo/Regular ammo set config")]
    public sealed class RegularAmmoSetConfig : AmmoSetConfig
    {
        public override AmmoType AmmoType => AmmoType.Regular;
    }
}