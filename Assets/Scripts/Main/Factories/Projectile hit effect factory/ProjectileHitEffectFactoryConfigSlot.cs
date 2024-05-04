using System;

using UnityEngine;

namespace SpaceAce.Main.Factories.ProjectileHitEffectFactories
{
    [Serializable]
    public sealed class ProjectileHitEffectFactoryConfigSlot
    {
        [SerializeField]
        private ProjectileHitEffectSkin _skin;

        [SerializeField]
        private GameObject _prefab;

        public ProjectileHitEffectSkin Skin => _skin;

        public GameObject Prefab => _prefab;
    }
}