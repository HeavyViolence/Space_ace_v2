using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories.ExplosionFactories
{
    [CreateAssetMenu(fileName = "Explosion factory config",
                     menuName = "Space ace/Configs/Factories/Explosion factory config")]
    public class ExplosionFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<ExplosionSlot> _explosions;

        public IEnumerable<ExplosionSlot> Explosions => _explosions;
    }
}