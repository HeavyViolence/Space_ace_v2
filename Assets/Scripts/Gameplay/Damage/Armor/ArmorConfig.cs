using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    [CreateAssetMenu(fileName = "Armor config",
                     menuName = "Space ace/Configs/Damage/Armor config")]
    public sealed class ArmorConfig : ScriptableObject
    {
        public const float MinInitialArmor = 0f;
        public const float MaxInitialArmor = 1_000f;

        [SerializeField, MinMaxSlider(MinInitialArmor, MaxInitialArmor)]
        private Vector2 _armor = new(MinInitialArmor, MaxInitialArmor);

        public float MinInitialValue => _armor.x;
        public float MaxInitialValue => _armor.y;
        public float RandomInitialValue => Random.Range(_armor.x, _armor.y);
    }
}