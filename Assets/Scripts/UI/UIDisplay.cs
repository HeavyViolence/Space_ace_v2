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

        public UIDisplay(VisualTreeAsset displayAsset, PanelSettings settings, Localizer localizer)
        {
            if (displayAsset == null) throw new ArgumentNullException();
            if (settings == null) throw new ArgumentNullException();

            Localizer = localizer ?? throw new ArgumentNullException();

            DisplayAsset = displayAsset;

            GameObject uiDisplay = new(DisplayHolderName);

            DisplayedDocument = uiDisplay.AddComponent<UIDocument>();
            DisplayedDocument.panelSettings = settings;
        }

        public abstract UniTask EnableAsync();
        public abstract void Disable();
    }
}