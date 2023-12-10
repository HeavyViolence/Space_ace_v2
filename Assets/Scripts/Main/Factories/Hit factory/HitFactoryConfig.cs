using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories
{
    [CreateAssetMenu(fileName = "Hit factory config",
                     menuName = "Space ace/Configs/Factories/Hit factory config")]
    public sealed class HitFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<HitSlot> _hitEffects;

        public IEnumerable<HitSlot> HitEffects => _hitEffects;
    }
}