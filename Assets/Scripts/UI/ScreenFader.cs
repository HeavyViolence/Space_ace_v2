using Cysharp.Threading.Tasks;
using SpaceAce.Architecture;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Localization;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public sealed class ScreenFader : UIDisplay, IInitializable, IDisposable
    {
        private const float FadingDurationFactor = 2f;

        protected override string DisplayHolderName => "Screen fader";

        private readonly MainMenuLoader _mainMenuLoader = null;
        private readonly LevelsLoader _levelsLoader = null;
        private readonly AnimationCurve _fadingCurve = null;

        public ScreenFader(VisualTreeAsset displayAsset,
                           PanelSettings settings,
                           UIAudio audio,
                           Localizer localizer,
                           MainMenuLoader mainMenuLoader,
                           LevelsLoader levelsLoader,
                           AnimationCurve fadingCurve) : base(displayAsset, settings, audio, localizer)
        {
            _mainMenuLoader = mainMenuLoader ?? throw new ArgumentNullException(nameof(mainMenuLoader),
                $"Attempted to pass an empty {typeof(MainMenuLoader)}!");

            _levelsLoader = levelsLoader ?? throw new ArgumentNullException(nameof(levelsLoader),
                $"Attempted to pass an empty {typeof(LevelsLoader)}!");

            if (fadingCurve == null)
                throw new ArgumentNullException(nameof(fadingCurve),
                    $"Attempted to pass an empty fading curve: {typeof(AnimationCurve)}!");

            _fadingCurve = fadingCurve;
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

        #region interfaces

        public void Initialize()
        {
            _mainMenuLoader.MainMenuLoadingStarted += MainMenuLoadingStartedEventHandler;
            _levelsLoader.LevelLoadingStarted += LevelLoadingStartedEventHandler;
        }

        public void Dispose()
        {
            _mainMenuLoader.MainMenuLoadingStarted -= MainMenuLoadingStartedEventHandler;
            _levelsLoader.LevelLoadingStarted -= LevelLoadingStartedEventHandler;
        }

        #endregion

        #region event handlers

        private void MainMenuLoadingStartedEventHandler(object sender, MainMenuLoadingStartedEventArgs e)
        {
            FadeInAndOut(e.LoadingDelay * FadingDurationFactor).Forget();
        }

        private void LevelLoadingStartedEventHandler(object sender, LevelLoadingStartedEventArgs e)
        {
            FadeInAndOut(e.LoadingDelay * FadingDurationFactor).Forget();
        }

        private async UniTaskVoid FadeInAndOut(float duration)
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

        #endregion
    }
}