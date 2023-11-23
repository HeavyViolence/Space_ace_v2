using System;

using UnityEngine;

using static UnityEngine.InputSystem.InputAction;

namespace SpaceAce.Gameplay.Controls
{
    public interface IGameControlsTransmitter
    {
        event EventHandler<CallbackContext> GoToPreviousMenu;
        event EventHandler<CallbackContext> OpenInventory;

        event EventHandler<CallbackContext> SelectNextAmmo;
        event EventHandler<CallbackContext> SelectPreviousAmmo;

        event EventHandler<CallbackContext> Fire;
        event EventHandler<CallbackContext> Ceasefire;

        Vector2 MovementDirection { get; }
        Vector3 MouseWorldPosition { get; }
    }
}