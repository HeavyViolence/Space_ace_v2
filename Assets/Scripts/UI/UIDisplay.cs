using Cysharp.Threading.Tasks;

using SpaceAce.Main.Localization;

using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public abstract class UIDisplay
    {
        protected readonly VisualTreeAsset DisplayAsset;
        protected readonly UIDocument DisplayedDocument;
        protected readonly Localizer Localizer;

        protected abstract string DisplayHolderName { get; }

        public bool Enabled { get; protected set; } = false;

        public UIDisplay(VisualTreeAsset displayAsset,
                         PanelSettings settings,
                         Localizer localizer)
        {
            if (displayAsset == null)
                throw new ArgumentNullException(nameof(displayAsset),
                    $"Attempted to pass an empty {typeof(VisualTreeAsset)} to display!");

            if (settings == null)
                throw new ArgumentNullException(nameof(settings),
                    $"Attempted to pass an empty {typeof(PanelSettings)} for the display!");

            DisplayAsset = displayAsset;

            GameObject uiDisplay = new(DisplayHolderName);

            DisplayedDocument = uiDisplay.AddComponent<UIDocument>();
            DisplayedDocument.panelSettings = settings;

            Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer),
                $"Attempted to pass an empty {typeof(Localizer)}!");
        }

        public abstract UniTask EnableAsync();

        public abstract void Disable();
    }
}