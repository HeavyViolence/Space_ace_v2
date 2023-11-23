using Cysharp.Threading.Tasks;

using System;

using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public interface ILevelSelectionDisplay
    {
        event EventHandler MainMenuButtonClicked;
        event EventHandler BattleButtonClicked;
        event EventHandler<LevelButtonCheckedEventArgs> LevelButtonChecked;
        event EventHandler LevelButtonUnchecked;
        event EventHandler<PointerOverEvent> PointerOver;

        bool Enabled { get; }

        UniTask EnableAsync();
        void Disable();
    }
}