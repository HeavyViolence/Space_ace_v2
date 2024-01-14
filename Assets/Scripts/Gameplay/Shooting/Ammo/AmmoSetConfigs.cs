using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Ammo set configs",
                     menuName = "Space ace/Configs/Shooting/Ammo/Ammo set configs")]
    public sealed class AmmoSetConfigs : ScriptableObject
    {
        [SerializeField]
        private RegularAmmoSetConfig _regularAmmoConfig;

        public RegularAmmoSetConfig RegularAmmoConfig => _regularAmmoConfig;

        [SerializeField]
        private StrangeAmmoSetConfig _strangeAmmoConfig;

        public StrangeAmmoSetConfig StrangeAmmoConfig => _strangeAmmoConfig;
    }
}