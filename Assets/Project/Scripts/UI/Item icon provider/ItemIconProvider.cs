using System;

using UnityEngine;

namespace SpaceAce.UI
{
    public sealed class ItemIconProvider
    {
        private readonly ItemIconProviderConfig _config;

        public ItemIconProvider(ItemIconProviderConfig config)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;
        }

        public Sprite GetItemIcon(object item) => _config.GetItemIcon(item);
    }
}