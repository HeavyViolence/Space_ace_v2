using SpaceAce.Gameplay.Items;

using UnityEngine;

namespace SpaceAce.UI
{
    [CreateAssetMenu(fileName = "Item quality to color converter config",
                     menuName = "Space ace/Configs/UI/Item quality to color converter config")]
    public sealed class ItemQualityToColorConverterConfig : ScriptableObject
    {
        [SerializeField]
        private Color _commonQualityColor;

        [SerializeField]
        private Color _uncommonQualityColor;

        [SerializeField]
        private Color _rareQualityColor;

        [SerializeField]
        private Color _exceptionalQualityColor;

        [SerializeField]
        private Color _exoticQualityColor;

        [SerializeField]
        private Color _epicQualityColor;

        [SerializeField]
        private Color _legendaryQualityColor;

        public Color GetQualityColor(Quality quality)
        {
            return quality switch
            {
                Quality.Common => _commonQualityColor,
                Quality.Uncommon => _uncommonQualityColor,
                Quality.Rare => _rareQualityColor,
                Quality.Exceptional => _exceptionalQualityColor,
                Quality.Exotic => _exoticQualityColor,
                Quality.Epic => _epicQualityColor,
                Quality.Legendary => _legendaryQualityColor,
                _ => _commonQualityColor
            };
        }
    }
}