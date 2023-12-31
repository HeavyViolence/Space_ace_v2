using SpaceAce.Gameplay.Inventories;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main.Factories;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Players
{
    [Serializable]
    public sealed class PlayerStartingAmmoConfig
    {
        public const int MinAmount = 1;
        public const int MaxAmount = 1_000;

        [SerializeField]
        private AmmoType _type = AmmoType.Regular;

        [SerializeField]
        private ItemSize _size = ItemSize.Medium;

        [SerializeField]
        private ItemQuality _quality = ItemQuality.Common;

        [SerializeField, Range(MinAmount, MaxAmount)]
        private int _amount = MinAmount;

        public AmmoFactoryRequest Request => new(_type, _size, _quality, _amount);
    }
}
