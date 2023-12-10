using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [Serializable]
    public sealed class HitSlot
    {
        [SerializeField]
        private HitStrength _strength;

        [SerializeField]
        private GameObject _hitPrefab;

        public HitStrength Strength => _strength;

        public GameObject Prefab => _hitPrefab;
    }
}