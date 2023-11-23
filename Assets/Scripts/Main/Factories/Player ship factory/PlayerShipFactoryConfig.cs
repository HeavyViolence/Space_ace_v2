using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [CreateAssetMenu(fileName = "Player ship factory config",
                     menuName = "Space ace/Configs/Factories/Player ship factory config")]
    public sealed class PlayerShipFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<PlayerShipSlot> _slots;

        public IEnumerable<KeyValuePair<PlayerShipType, GameObject>> PlayerShips
        {
            get
            {
                Dictionary<PlayerShipType, GameObject> playerShips = new();

                foreach (var slot in _slots)
                    playerShips.Add(slot.ShipType, slot.Prefab);

                return playerShips;
            }
        }
    }
}