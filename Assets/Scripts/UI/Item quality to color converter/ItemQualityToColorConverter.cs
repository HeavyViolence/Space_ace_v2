using SpaceAce.Gameplay.Inventories;

using System;

using UnityEngine;

namespace SpaceAce.UI
{
    public sealed class ItemQualityToColorConverter
    {
        private readonly ItemQualityToColorConverterConfig _config;

        public ItemQualityToColorConverter(ItemQualityToColorConverterConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config),
                    $"Attempted to pass an empty {typeof(ItemQualityToColorConverterConfig)}!");
        }

        public Color32 GetQualityColor(ItemQuality quality) => _config.GetQualityColor(quality);
    }
}