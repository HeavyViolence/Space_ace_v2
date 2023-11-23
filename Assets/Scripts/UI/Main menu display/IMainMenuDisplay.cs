using Cysharp.Threading.Tasks;

using System;

using UnityEngine.UIElements;

namespace SpaceAce.UI
{
    public interface IMainMenuDisplay
    {
        event EventHandler PlayButtonClicked;
        event EventHandler InventoryButtonClicked;
        event EventHandler ArmoryButtonClicked;
        event EventHandler SettingsButtonClicked;
        event EventHandler StatisticsButtonClicked;
        event EventHandler CreditsButtonClicked;
        event EventHandler CheatsButtonClicked;
        event EventHandler<PointerOverEvent> PointerOver;

        bool Enabled { get; }

        UniTask EnableAsync();
        void Disable();
    }
}