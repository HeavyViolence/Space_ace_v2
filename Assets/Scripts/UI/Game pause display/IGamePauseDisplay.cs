using Cysharp.Threading.Tasks;

using System;

using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public interface IGamePauseDisplay
    {
        event EventHandler ResumeButtonClicked;
        event EventHandler InventoryButtonClicked;
        event EventHandler SettingsButtonClicked;
        event EventHandler MainMenuButtonClicked;
        event EventHandler<PointerOverEvent> PointerOver;

        bool Enabled { get; }

        UniTask EnableAsync();
        void Disable();
    }
}