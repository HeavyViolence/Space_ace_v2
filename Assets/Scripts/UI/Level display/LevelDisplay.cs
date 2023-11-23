using Cysharp.Threading.Tasks;

using SpaceAce.Main.Localization;

using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public sealed class LevelDisplay : UIDisplay, ILevelDisplay
    {
        protected override string DisplayHolderName => "Level display";

        public LevelDisplay(VisualTreeAsset displayAsset,
                            PanelSettings settings,
                            ILocalizer localizer) : base(displayAsset,
                                                         settings,
                                                         localizer) { }

        public override async UniTask EnableAsync()
        {
            DisplayedDocument.visualTreeAsset = DisplayAsset;

            await UniTask.NextFrame();

            Enabled = true;
        }

        public override void Disable()
        {
            DisplayedDocument.visualTreeAsset = null;
            Enabled = false;
        }
    }
}