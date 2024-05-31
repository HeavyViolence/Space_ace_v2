using Cysharp.Threading.Tasks;

using SpaceAce.Main.Localization;

using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI.Displays
{
    public abstract class UIDisplay
    {
        public event EventHandler Enabled, Disabled;

        protected readonly VisualTreeAsset DisplayAsset;
        protected readonly UIDocument DisplayedDocument;
        protected readonly Localizer Localizer;

        protected abstract string DisplayHolderName { get; }

        public bool Active { get; protected set; } = false;

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

        public virtual async UniTask EnableAsync()
        {
            await UniTask.Yield();

            Active = true;
            Enabled?.Invoke(this, EventArgs.Empty);
        }

        public virtual async UniTask DisableAsync()
        {
            await UniTask.Yield();

            Active = false;
            Disabled?.Invoke(this, EventArgs.Empty);
        }
    }
}