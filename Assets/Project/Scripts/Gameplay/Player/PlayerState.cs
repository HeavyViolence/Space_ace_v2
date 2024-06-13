using SpaceAce.Gameplay.Items;
using SpaceAce.Main.Factories.PlayerShipFactories;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SpaceAce.Gameplay.Players
{
    public sealed class PlayerState
    {
        public static PlayerState Default => new(100f, 0f, PlayerShipType.TempestMk1, Enumerable.Empty<ItemSavableState>());

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
            InventoryContent = inventoryContent ?? throw new ArgumentNullException();
        }
    }
}