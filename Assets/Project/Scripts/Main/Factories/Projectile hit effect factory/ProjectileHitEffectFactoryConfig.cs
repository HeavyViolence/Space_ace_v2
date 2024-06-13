using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Main.Factories.ProjectileHitEffectFactories
{
    [CreateAssetMenu(fileName = "Projectile hit effect factory config",
                     menuName = "Space ace/Configs/Factories/Projectile hit effect factory config")]
    public sealed class ProjectileHitEffectFactoryConfig : ScriptableObject
    {
        [SerializeField]
        private List<ProjectileHitEffectFactoryConfigSlot> _hitEffects;

        public IEnumerable<KeyValuePair<ProjectileHitEffectSkin, GameObject>> GetHitEffectsPrefabs()
        {
            Dictionary<ProjectileHitEffectSkin, GameObject> prefabs = new(_hitEffects.Count);

            foreach (var hitEffect in _hitEffects) prefabs.Add(hitEffect.Skin, hitEffect.Prefab);

            return prefabs;
        }
    }
}