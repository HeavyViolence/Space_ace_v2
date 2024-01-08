using SpaceAce.Gameplay.Inventories;
using SpaceAce.Gameplay.Shooting.Ammo;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    public readonly struct AmmoFactoryRequest
    {
        public AmmoType Type { get; }
        public ItemSize Size { get; }
        public ItemQuality Quality { get; }
        public int Amount { get; }

        public AmmoFactoryRequest(AmmoType type,
                                  ItemSize size,
                                  ItemQuality quality,
                                  int amount)
        {
            Type = type;
            Size = size;
            Quality = quality;
            Amount = Mathf.Clamp(amount, 0, int.MaxValue);
        }
    }
}