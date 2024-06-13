using NaughtyAttributes;

using UnityEngine;

namespace SpaceAce.Gameplay.Items
{
    public abstract class ItemConfig : ScriptableObject
    {
        public const float MinPrice = 0.01f;
        public const float MaxPrice = 10_000f;

        [SerializeField, MinMaxSlider(MinPrice, MaxPrice)]
        private Vector2 _price = new(MinPrice, MaxPrice);

        public Vector2 Price => _price;
    }
}