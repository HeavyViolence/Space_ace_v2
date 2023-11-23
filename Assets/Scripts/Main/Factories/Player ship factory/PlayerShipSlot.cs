using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [Serializable]
    public sealed class PlayerShipSlot
    {
        public PlayerShipType ShipType;
        public GameObject Prefab;
    }
}