using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories.PlayerShipFactories
{
    [CreateAssetMenu(fileName = "Player ship factory config",
                     menuName = "Space ace/Configs/Factories/Player ship factory config")]
    public sealed class PlayerShipFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<PlayerShipSlot> _slots;

        public IEnumerable<KeyValuePair<PlayerShipType, GameObject>> GetPrefabs()
        {
            Dictionary<PlayerShipType, GameObject> _prefabs = new(_slots.Count);

            foreach(PlayerShipSlot slot in _slots)
            {
                if (slot.Prefab == null) throw new ArgumentNullException();
                _prefabs.Add(slot.Type, slot.Prefab);
            }

            return _prefabs;
        }
    }
}