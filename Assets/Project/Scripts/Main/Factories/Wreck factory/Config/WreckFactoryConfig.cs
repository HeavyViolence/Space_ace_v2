using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories.WreckFactories
{
    [CreateAssetMenu(fileName = "Wreck factory config",
                     menuName = "Space ace/Configs/Factories/Wreck factory config")]
    public sealed class WreckFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<WreckPrefabSlot> _wrecks;

        public int PrefabsCount => _wrecks.Count;

        public IEnumerable<KeyValuePair<WreckType, GameObject>> GetPrefabs()
        {
            Dictionary<WreckType, GameObject> prefabs = new(_wrecks.Count);

            foreach (var entry in _wrecks)
            {
                if (entry.Prefab == null) throw new ArgumentNullException();
                prefabs.Add(entry.Type, entry.Prefab);
            }

            return prefabs;
        }
    }
}