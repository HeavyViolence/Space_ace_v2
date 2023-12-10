using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [Serializable]
    public sealed class PlayerShipSlot
    {
        [SerializeField]
        private PlayerShipType _shipType;

        [SerializeField]
        private GameObject _prefab;

        public PlayerShipType ShipType => _shipType;

        public GameObject Prefab => _prefab;
    }
}