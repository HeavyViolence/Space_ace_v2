using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    [CreateAssetMenu(fileName = "Level selection display assets",
                     menuName = "Space ace/Configs/UI/Level selection display assets")]
    public sealed class LevelSelectionDisplayAssets : ScriptableObject
    {
        [SerializeField]
        private VisualTreeAsset _display;

        [SerializeField]
        private VisualTreeAsset _levelButton;

        [SerializeField]
        private PanelSettings _settings;

        public VisualTreeAsset Display => _display;
        public VisualTreeAsset LevelButton => _levelButton;
        public PanelSettings Settings => _settings;
    }
}