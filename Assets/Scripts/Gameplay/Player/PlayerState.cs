using SpaceAce.Gameplay.Items;
using SpaceAce.Main.Factories;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Players
{
    public sealed class PlayerState
    {
        public static PlayerState Default => new(0f, 0f, PlayerShipType.Ship1Mk1, null);

        public float Credits { get; }
        public float Experience { get; }
        public PlayerShipType SelectedShip { get; }
        public IEnumerable<ItemSavableState> InventoryContent { get; }

        public PlayerState(float credits,
                           float experience,
                           PlayerShipType selectedShip,
                           IEnumerable<ItemSavableState> inventoryContent)
        {
            Credits = Mathf.Clamp(credits, 0f, float.MaxValue);
            Experience = Mathf.Clamp(experience, 0f, float.MaxValue);
            SelectedShip = selectedShip;
            InventoryContent = inventoryContent;
        }
    }
}