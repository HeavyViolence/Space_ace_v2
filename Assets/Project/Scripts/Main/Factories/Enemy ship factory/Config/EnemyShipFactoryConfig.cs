using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories.EnemyShipFactories
{
    [CreateAssetMenu(fileName = "Enemy factory config",
                     menuName = "Space ace/Configs/Factories/Enemy ship factory config")]
    public sealed class EnemyShipFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<EnemyShipSlot> _slots;

        public IEnumerable<KeyValuePair<EnemyShipType, GameObject>> GetPrefabs()
        {
            Dictionary<EnemyShipType, GameObject> prefabs = new(_slots.Count);

            foreach (var slot in _slots)
                prefabs.Add(slot.Type, slot.Prefab);

            return prefabs;
        }
    }
}