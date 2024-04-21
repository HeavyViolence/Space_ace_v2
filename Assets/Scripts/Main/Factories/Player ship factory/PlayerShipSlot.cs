using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [Serializable]
    public sealed class PlayerShipSlot
    {
        [SerializeField]
        private PlayerShipType _type;

        [SerializeField]
        private GameObject _prefab;

        public PlayerShipType Type => _type;

        public GameObject Prefab => _prefab;
    }
}