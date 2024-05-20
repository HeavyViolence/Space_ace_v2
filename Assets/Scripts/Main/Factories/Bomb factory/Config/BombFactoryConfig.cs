using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories.BombFactories
{
    [CreateAssetMenu(fileName = "Bomb factory config",
                     menuName = "Space ace/Configs/Factories/Bomb factory config")]
    public sealed class BombFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<BombPrefabSlot> _bombs;

        public int PrefabsCount => _bombs.Count;

        public IEnumerable<KeyValuePair<BombSize, GameObject>> GetPrefabs()
        {
            Dictionary<BombSize, GameObject> prefabs = new(_bombs.Count);

            foreach (var entry in _bombs)
            {
                if (entry.Prefab == null) throw new ArgumentNullException();
                prefabs.Add(entry.Type, entry.Prefab);
            }

            return prefabs;
        }
    }
}