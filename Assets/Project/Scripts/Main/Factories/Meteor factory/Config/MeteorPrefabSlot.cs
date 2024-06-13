using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.MeteorFactories
{
    [Serializable]
    public sealed class MeteorPrefabSlot
    {
        [SerializeField]
        private MeteorType _type;

        [SerializeField]
        private GameObject _prefab;

        public MeteorType Type => _type;

        public GameObject Prefab => _prefab;
    }
}