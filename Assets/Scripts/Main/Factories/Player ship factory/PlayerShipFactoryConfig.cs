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

        public IEnumerable<PlayerShipSlot> Content => _slots;
    }
}