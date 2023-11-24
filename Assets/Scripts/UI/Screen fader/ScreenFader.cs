using Cysharp.Threading.Tasks;

using SpaceAce.Main.Localization;

using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI
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
            if (uiSettings == null)
                throw new ArgumentNullException(nameof(uiSettings),
                    $"Attempted to pass an empty fading curve: {typeof(AnimationCurve)}!");

            _fadingCurve = uiSettings.FadingCurve;
        }

        public override async UniTask EnableAsync()
        {
            if (Enabled == true) return;

            DisplayedDocument.visualTreeAsset = DisplayAsset;
            DisplayedDocument.rootVisualElement.style.opacity = 0;

            Enabled = true;

            await UniTask.NextFrame();
        }

        public override void Disable()
        {
            if (Enabled == false) return;

            DisplayedDocument.visualTreeAsset = null;
            Enabled = false;
        }

        public async UniTask FadeInAndOutAsync(float duration)
        {
            await EnableAsync();

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float opacity = _fadingCurve.Evaluate(timer / duration);
                DisplayedDocument.rootVisualElement.style.opacity = opacity;

                await UniTask.NextFrame();
            }

            Disable();
        }
    }
}