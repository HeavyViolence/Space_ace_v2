using UnityEngine;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    [CreateAssetMenu(fileName = "Ammo configs",
                     menuName = "Space ace/Configs/Shooting/Ammo/Ammo configs")]
    public sealed class AmmoConfigs : ScriptableObject
    {
        [SerializeField]
        private RegularAmmoConfig _regularAmmoConfig;

        public RegularAmmoConfig RegularAmmoConfig => _regularAmmoConfig;

        [SerializeField]
        private StrangeAmmoConfig _strangeAmmoConfig;

        public StrangeAmmoConfig StrangeAmmoConfig => _strangeAmmoConfig;
    }
}