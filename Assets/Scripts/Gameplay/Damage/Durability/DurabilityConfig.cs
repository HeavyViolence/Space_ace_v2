using NaughtyAttributes;

using SpaceAce.Main.Factories;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    [CreateAssetMenu(fileName = "Durability config",
                     menuName = "Space ace/Configs/Damage/Durability config")]
    public sealed class DurabilityConfig : ScriptableObject
    {
        public const float MinInitialDurability = 1f;
        public const float MaxInitialDurability = 1_000_000f;

        public const float MinInitialRegen = 0f;
        public const float MaxInitialRegen = 1_000f;

        [SerializeField] private Sprite _icon;

        public Sprite Icon => _icon;

        [SerializeField, MinMaxSlider(MinInitialDurability, MaxInitialDurability), Space]
        private Vector2 _durability = new(MinInitialDurability, MaxInitialDurability);

        public float MinInitialValue => _durability.x;
        public float MaxInitialValue => _durability.y;
        public float RandomInitialValue => Random.Range(_durability.x, _durability.y);

        [SerializeField, MinMaxSlider(MinInitialRegen, MaxInitialRegen)]
        private Vector2 _regen = new(MinInitialRegen, MaxInitialRegen);

        public float MinInitialValueRegen => _regen.x;
        public float MaxInitialValueRegen => _regen.y;
        public float RandomInitialValueRegen => Random.Range(_regen.x, _regen.y);
    }
}