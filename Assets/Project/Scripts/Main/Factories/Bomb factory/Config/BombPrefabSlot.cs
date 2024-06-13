using SpaceAce.Main.Factories.BombFactories;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [Serializable]
    public sealed class BombPrefabSlot
    {
        [SerializeField]
        private BombSize _type;

        [SerializeField]
        private GameObject _prefab;

        public BombSize Type => _type;

        public GameObject Prefab => _prefab;
    }
}