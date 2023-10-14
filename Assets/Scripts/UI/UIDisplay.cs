using Cysharp.Threading.Tasks;
using SpaceAce.Main.Localization;
using SpaceAce.Main.Audio;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public abstract class UIDisplay
    {
        protected readonly VisualTreeAsset DisplayAsset;
        protected readonly UIDocument DisplayedDocument;
        protected readonly UIAudio UIAudio;
        protected readonly Localizer Localizer;

        protected abstract string DisplayHolderName { get; }

        public bool Enabled { get; protected set; } = false;

        public UIDisplay(VisualTreeAsset displayAsset, PanelSettings settings, UIAudio audio, Localizer localizer)
        {
            if (displayAsset == null) throw new ArgumentNullException(nameof(displayAsset),
                $"Attempted to pass an empty {typeof(VisualTreeAsset)} to display!");

            if (settings == null) throw new ArgumentNullException(nameof(settings),
                $"Attempted to pass an empty {typeof(PanelSettings)} for the display!");

            if (audio == null) throw new ArgumentNullException(nameof(audio),
                $"Attempted to pass an empty {typeof(UIAudio)} to play!");

            Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer),
                $"Attempted to pass an empty {typeof(Localizer)}!");

            DisplayAsset = displayAsset;
            UIAudio = audio;

            GameObject uiDisplay = new(DisplayHolderName);

            DisplayedDocument = uiDisplay.AddComponent<UIDocument>();
            DisplayedDocument.panelSettings = settings;
        }

        public abstract UniTaskVoid EnableAsync();

        public abstract void Disable();
    }
}