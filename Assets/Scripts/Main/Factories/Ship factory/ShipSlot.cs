using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [Serializable]
    public sealed class ShipSlot
    {
        [SerializeField]
        private ShipType _shipType;

        [SerializeField]
        private GameObject _prefab;

        public ShipType ShipType => _shipType;

        public GameObject Prefab => _prefab;
    }
}