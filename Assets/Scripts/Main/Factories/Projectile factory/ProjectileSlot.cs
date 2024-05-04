using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.ProjectileFactories
{
    [Serializable]
    public sealed class ProjectileSlot
    {
        [SerializeField]
        private ProjectileSkin _skin;

        [SerializeField]
        private GameObject _prefab;

        public ProjectileSkin Skin => _skin;

        public GameObject Prefab => _prefab;
    }
}