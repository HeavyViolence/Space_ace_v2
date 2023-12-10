using SpaceAce.Main.Audio;

using System;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [Serializable]
    public sealed class ExplosionSlot
    {
        [SerializeField]
        private ExplosionSize _size;

        [SerializeField]
        private GameObject _prefab;

        [SerializeField]
        private AudioCollection _audio;

        public ExplosionSize Size => _size;

        public GameObject Prefab => _prefab;

        public AudioCollection Audio => _audio;
    }
}