using SpaceAce.Gameplay.Inventories;

using UnityEngine;

namespace SpaceAce.UI
{
    [CreateAssetMenu(fileName = "Item quality to color converter config",
                     menuName = "Space ace/Configs/UI/Item quality to color converter config")]
    public sealed class ItemQualityToColorConverterConfig : ScriptableObject
    {
        [SerializeField]
        private Color32 _poorQualityColor;

        [SerializeField]
        private Color32 _commonQualityColor;

        [SerializeField]
        private Color32 _uncommonQualityColor;

        [SerializeField]
        private Color32 _rareQualityColor;

        [SerializeField]
        private Color32 _exoticQualityColor;

        [SerializeField]
        private Color32 _epicQualityColor;

        [SerializeField]
        private Color32 _legendaryQualityColor;

        public Color32 GetQualityColor(ItemQuality quality)
        {
            return quality switch
            {
                ItemQuality.Poor => _poorQualityColor,
                ItemQuality.Common => _commonQualityColor,
                ItemQuality.Uncommon => _uncommonQualityColor,
                ItemQuality.Rare => _rareQualityColor,
                ItemQuality.Exotic => _exoticQualityColor,
                ItemQuality.Epic => _epicQualityColor,
                ItemQuality.Legendary => _legendaryQualityColor,
                _ => _poorQualityColor
            };
        }
    }
}