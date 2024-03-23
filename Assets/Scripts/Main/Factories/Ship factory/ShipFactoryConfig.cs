using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [CreateAssetMenu(fileName = "Ship factory config",
                     menuName = "Space ace/Configs/Factories/Ship factory config")]
    public sealed class ShipFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<ShipSlot> _slots;

        public IEnumerable<ShipSlot> Content => _slots;
    }
}