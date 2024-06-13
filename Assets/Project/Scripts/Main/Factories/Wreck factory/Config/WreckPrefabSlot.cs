using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.WreckFactories
{
    [Serializable]
    public sealed class WreckPrefabSlot
    {
        [SerializeField]
        private WreckType _type;

        [SerializeField]
        private GameObject _prefab;

        public WreckType Type => _type;

        public GameObject Prefab => _prefab;
    }
}