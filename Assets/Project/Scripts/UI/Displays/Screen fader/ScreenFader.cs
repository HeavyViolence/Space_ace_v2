using Cysharp.Threading.Tasks;

using SpaceAce.Main.Localization;

using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI.Displays
{
    public sealed class ScreenFader : UIDisplay
    {
        protected override string DisplayHolderName => "Screen fader";

        private readonly AnimationCurve _fadingCurve = null;

        public ScreenFader(VisualTreeAsset displayAsset,
                           PanelSettings settings,
                           UISettings uiSettings,
                           Localizer localizer) : base(displayAsset,
                                                       settings,
                                                       localizer)
        {
            if (uiSettings == null) throw new ArgumentNullException();
            _fadingCurve = uiSettings.FadingCurve;
        }

        public override async UniTask EnableAsync()
        {
            if (Active == true) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;
            DisplayedDocument.rootVisualElement.style.opacity = 0;

            await base.EnableAsync();
        }

        public override async UniTask DisableAsync()
        {
            if (Active == false || DisplayedDocument == null) return;

            DisplayedDocument.visualTreeAsset = null;

            await base.DisableAsync();
        }

        public async UniTask FadeInAndOutAsync(float duration)
        {
            await EnableAsync();

            float timer = 0f;

            while (timer < duration)
            {
                if (DisplayedDocument == null) break;

                timer += Time.deltaTime;

                float opacity = _fadingCurve.Evaluate(timer / duration);
                DisplayedDocument.rootVisualElement.style.opacity = opacity;

                await UniTask.NextFrame();
            }

            await DisableAsync();
        }
    }
}