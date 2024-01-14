using SpaceAce.Gameplay.Items;

using UnityEngine;

namespace SpaceAce.UI
{
    [CreateAssetMenu(fileName = "Item quality to color converter config",
                     menuName = "Space ace/Configs/UI/Item quality to color converter config")]
    public sealed class ItemQualityToColorConverterConfig : ScriptableObject
    {
        [SerializeField]
        private Color32 _commonQualityColor;

        [SerializeField]
        private Color32 _uncommonQualityColor;

        [SerializeField]
        private Color32 _rareQualityColor;

        [SerializeField]
        private Color32 _exceptionalQualityColor;

        [SerializeField]
        private Color32 _exoticQualityColor;

        [SerializeField]
        private Color32 _epicQualityColor;

        [SerializeField]
        private Color32 _legendaryQualityColor;

        public Color32 GetQualityColor(Quality quality)
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