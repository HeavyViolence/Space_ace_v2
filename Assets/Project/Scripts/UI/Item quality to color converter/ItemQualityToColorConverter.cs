using SpaceAce.Gameplay.Items;

using System;

using UnityEngine;

namespace SpaceAce.UI
{
    public sealed class ItemQualityToColorConverter
    {
        private readonly ItemQualityToColorConverterConfig _config;

        public ItemQualityToColorConverter(ItemQualityToColorConverterConfig config)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;
        }

        public Color GetQualityColor(Quality quality) => _config.GetQualityColor(quality);
    }
}