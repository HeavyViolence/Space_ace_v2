using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories.MeteorFactories
{
    [CreateAssetMenu(fileName = "Meteor factory config",
                     menuName = "Space ace/Configs/Factories/Meteor factory config")]
    public sealed class MeteorFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<MeteorPrefabSlot> _meteors;

        public int PrefabsCount => _meteors.Count;

        public IEnumerable<KeyValuePair<MeteorType, GameObject>> GetPrefabs()
        {
            Dictionary<MeteorType, GameObject> prefabs = new(_meteors.Count);

            foreach (var entry in _meteors)
            {
                if (entry.Prefab == null) throw new ArgumentNullException();
                prefabs.Add(entry.Type, entry.Prefab);
            }

            return prefabs;
        }
    }
}