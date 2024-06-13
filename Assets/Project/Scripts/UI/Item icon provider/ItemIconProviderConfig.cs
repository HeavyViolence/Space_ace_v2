using SpaceAce.Gameplay.Shooting.Ammo;

using System;

using UnityEngine;

namespace SpaceAce.UI
{
    [CreateAssetMenu(menuName = "Space ace/Configs/UI/Item icon provider config",
                 fileName = "Item icon provider config")]
    public sealed class ItemIconProviderConfig : ScriptableObject
    {
        [SerializeField]
        private Sprite _noItemIcon;

        [SerializeField]
        private Sprite _ammoSetIcon;

        public Sprite GetItemIcon(object item)
        {
            if (item is null) throw new ArgumentNullException();

            if (item is AmmoSet) return _ammoSetIcon;

            return _noItemIcon;
        }
    }
}