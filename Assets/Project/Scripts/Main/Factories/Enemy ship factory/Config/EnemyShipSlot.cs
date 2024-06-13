using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.EnemyShipFactories
{
    [Serializable]
    public sealed class EnemyShipSlot
    {
        [SerializeField]
        private EnemyShipType _type;

        public EnemyShipType Type => _type;

        [SerializeField]
        private GameObject _prefab;

        public GameObject Prefab => _prefab;
    }
}