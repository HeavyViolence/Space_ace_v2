using NaughtyAttributes;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    [CreateAssetMenu(fileName = "Armor config",
                     menuName = "Space ace/Configs/Damage/Armor config")]
    public sealed class ArmorConfig : ScriptableObject
    {
        #region armor

        public const float MinInitialArmor = 0f;
        public const float MaxInitialArmor = 1_000f;

        [SerializeField, MinMaxSlider(MinInitialArmor, MaxInitialArmor)]
        private Vector2 _armor = new(MinInitialArmor, MaxInitialArmor);

        public float MinInitialValue => _armor.x;
        public float MaxInitialValue => _armor.y;
        public float RandomInitialValue => UnityEngine.Random.Range(_armor.x, _armor.y);

        #endregion

        #region damage falloff

        [SerializeField, Tooltip("x: damage / armor, y: damage factor")]
        private AnimationCurve _damageFalloff;

        public float GetDamageFalloffFactor(float damage, float armor)
        {
            if (damage <= 0f) throw new ArgumentOutOfRangeException();
            if (armor < 0f) throw new ArgumentOutOfRangeException();

            return armor == 0f ? 1f : _damageFalloff.Evaluate(damage / armor);
        }

        #endregion
    }
}