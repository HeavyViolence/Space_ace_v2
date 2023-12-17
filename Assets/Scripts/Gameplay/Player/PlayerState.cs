using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main.Factories;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Players
{
    public sealed class PlayerState
    {
        public PlayerState Default => new(0f, 0f, PlayerShipType.Ship1Mk1, new List<ItemSnapshot>());

        public float Credits { get; }
        public float Experience { get; }
        public PlayerShipType SelectedShip { get; }
        public IEnumerable<ItemSnapshot> InventorySnapshot { get; }

        public PlayerState(float credits,
                           float experience,
                           PlayerShipType selectedShip,
                           IEnumerable<ItemSnapshot> inventorySnapshot)
        {
            Credits = Mathf.Clamp(credits, 0f, float.MaxValue);
            Experience = Mathf.Clamp(experience, 0f, float.MaxValue);
            SelectedShip = selectedShip;

            InventorySnapshot = inventorySnapshot ?? throw new ArgumentNullException(nameof(inventorySnapshot),
                "Attempted to pass an empty inventory snapshot!");
        }
    }
}