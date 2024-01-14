using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main.Factories;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Players
{
    [Serializable]
    public sealed class PlayerStartingAmmoConfig
    {
        [SerializeField]
        private AmmoType _type = AmmoType.Regular;

        [SerializeField]
        private Size _size = Size.Medium;

        [SerializeField]
        private Quality _quality = Quality.Common;

        public AmmoFactoryRequest Request => new(_type, _size, _quality);
    }
}
